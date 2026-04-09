using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Game.Base;
using Game.CameraSystem;
using Game.Framework;
using Game.Hud;
using Game.Soul;
using KanKikuchi.AudioManager;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Player
{
    public enum AttackState
    {
        Idle,
        Attacking,
        ComboQueued,
        Cooldown
    }

    [RequireComponent(typeof(PlayerStateHandler))]
    public class PlayerAttackCtrl : MonoBehaviour, IDamageable
    {
        private int? _insId;
        public int InsId => _insId ??= gameObject.GetEntityId();

        public PlayerPropManager PropManager => PlayerPropManager.Instance;

        [Header("Weapon")]
        public SerializedDictionary<WeaponHandType, Transform> weaponTransform;
        private Weapon currentWeapon;
        private WeaponHitbox currentWeaponHitbox;

        private PlayerInputHandler Input => PlayerInputHandler.Instance;
        private PlayerStateHandler stateHandler;
        private CharacterController controller;

        [Header("Attack Timings")]
        public float normalAttackDelay = 0.1f;
        public float normalHitWindow = 0.18f;
        public float skillHitRadius = 3f;
        public float skillHitDelay = 0.15f;
        public float skillHitWindow = 0.35f;

        [Header("Parry Settings")]
        [SerializeField] private float parryWindow = 0.25f;
        [SerializeField] private float parryCooldown = 0.6f;

        [Header("Dodge Settings")]
        [SerializeField] private float invulDuration = 0.35f;
        [SerializeField] private float perfectDodgeWindow = 0.3f; // 敌人攻击命中前的时间窗
        [SerializeField] private float dodgeDistance = 2f;     // 闪避水平距离
        [SerializeField] private float dodgeHeight = 0.4f;     // 闪避高度抬升
        [SerializeField] private float dodgeDuration = 0.25f;  // 动画持续时间
        [SerializeField] private AnimationCurve dodgeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


        private bool isParrying;
        private bool isInvulnerable;// 受到伤害后是否处于无敌状态
        private float lastParryTime;

        private float lastIncomingAttackTime = -99999f;
        private float lastAttackETA = 99999f;

        public event Action<GameObject> OnParrySuccess;
        public event UnityAction OnPerfectDodge;

        [Header("Hurt Setttings")]
        public float hurtInvulDuration = 1.35f;
        public event Action<float, int> OnHurt;

        private AttackState attackState = AttackState.Idle;
        private CancellationTokenSource attackCTS;

        private void Awake()
        {
            stateHandler = GetComponent<PlayerStateHandler>();
            controller = GetComponent<CharacterController>();
            EventQueueSystem.AddListener<SendDamageEvent>(DamageEventHandler);
        }

        private void Start()
        {
            GameManager.stageManager.AddOnePlayer(InsId, gameObject);
            EnterLevel(GameManager.Instance.LevelIdx == 0);
        }

        public void EnterLevel(bool isFirstLevel)
        {
            PropManager.Init(new PlayerData(GameManager.Instance.gameData.playerConfig.maxSwordPoint));
            EquipWeapon(GameManager.Instance.gameData.playerConfig.initWeaponId,isFirstLevel);
            _ = SwordSoulManager.Instance.ApplyInherentSoulsAsync();
            EventQueueSystem.QueueEvent(new PlayerEnterLevelEvent(PropManager.Prop));
        }

        public void EquipWeapon(int weaponId,bool isFirstLevel)
        {
            if (currentWeapon != null) return;
            var data = GameManager.Instance.gameData.playerWeaponDatas[weaponId];

            currentWeapon = GameObjectPool.Instance.GetObj(data.prefab, weaponTransform[data.handType]).GetComponent<Weapon>();
            currentWeapon.transform.ResetLocal();
            currentWeapon.InitWeapon(weaponId);
            if (isFirstLevel)
            {
                PropManager.AddProp(currentWeapon.AddProp);
                UIMessageSystem.Instance.AddMessage("装备武器:攻击力+100，生命值+100");
            }

            currentWeaponHitbox = currentWeapon.GetComponentInChildren<WeaponHitbox>();
            if (currentWeaponHitbox == null)
            {
                var go = new GameObject("WeaponHitbox");
                go.transform.SetParent(currentWeapon.transform, false);
                var col = go.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 0.6f;
                currentWeaponHitbox = go.AddComponent<WeaponHitbox>();
            }
            currentWeaponHitbox.Initialize(this);
            currentWeaponHitbox.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (currentWeapon == null) return;

            if (Input.AttackPressed)
            {
                switch (attackState)
                {
                    case AttackState.Idle:
                        HandleNormalAttackAsync().Forget();
                        break;
                    case AttackState.Attacking:
                        attackState = AttackState.ComboQueued;
                        break;
                }
            }
            else if (Input.SkillPressed && PropManager.CanUseSkill())
            {
                StartSkillAttackAsync().Forget();
            }
            else if (Input.ParryPressed)
            {
                if(TryParry())
                {
                    stateHandler.State = PlayerAnimatorState.ParrySuccess;
                }
                else
                {
                    stateHandler.State = PlayerAnimatorState.Parry;
                }
            }
            else if (Input.DodgePressed)
            {
                TryDodgeAsync(invulDuration).Forget();
            }
        }

        #region Normal Attack (UniTask)
        private async UniTaskVoid HandleNormalAttackAsync()
        {
            attackCTS?.Cancel();
            attackCTS = new CancellationTokenSource();

            attackState = AttackState.Attacking;
            stateHandler.State = PlayerAnimatorState.Attack;
            try
            {
                currentWeapon?.NormalAttack();
            }
            catch { }

            await UniTask.Delay(TimeSpan.FromSeconds(normalAttackDelay), cancellationToken: attackCTS.Token);
            currentWeaponHitbox?.Activate(normalHitWindow, false);
            await UniTask.Delay(TimeSpan.FromSeconds(normalHitWindow), cancellationToken: attackCTS.Token);

            if (attackState == AttackState.ComboQueued)
            {
                attackState = AttackState.Idle;
                await UniTask.Yield(); // 确保动画有1帧间隔
                HandleNormalAttackAsync().Forget();
                return;
            }

            attackState = AttackState.Cooldown;
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: attackCTS.Token);
            attackState = AttackState.Idle;
        }
        #endregion

        #region Skill (Area Attack)
        public async UniTaskVoid StartSkillAttackAsync()
        {
            // 中断当前攻击
            attackCTS?.Cancel();
            attackCTS = new CancellationTokenSource();

            attackState = AttackState.Attacking;
            stateHandler.State = PlayerAnimatorState.Skill;

            try
            {
                //  播放技能动画、特效、音效
                currentWeapon?.SpecialAttack(); // 可选：在 Weapon 类中定义
                Debug.Log("[SkillAttack] Start skill animation");

                //  前摇等待（例如 skillHitDelay = 0.35f）
                await UniTask.Delay(TimeSpan.FromSeconds(skillHitDelay), cancellationToken: attackCTS.Token);

                //  检测范围内的敌人
                var hits = Physics.OverlapSphere(transform.position, skillHitRadius);
                var hitTargets = new List<GameObject>();
                foreach (var c in hits)
                {
                    if (c.gameObject == gameObject) continue;

                    var dmgable = c.GetComponentInParent<IDamageable>();
                    if (dmgable != null)
                    {
                        hitTargets.Add(c.gameObject);
                    }
                }

                //  计算并发送伤害事件
                float damage = PropManager.CalSkillAttackDamaage();
                PropManager.RemoveSwordPoint(3);
                EventQueueSystem.QueueEvent(new SendDamageEvent(InsId, hitTargets, damage));

                Debug.Log($"[SkillAttack] Damage {damage} applied to {hitTargets.Count} targets.");

                //  保持命中窗口一段时间
                await UniTask.Delay(TimeSpan.FromSeconds(skillHitWindow), cancellationToken: attackCTS.Token);

                //  技能冷却阶段
                attackState = AttackState.Cooldown;
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: attackCTS.Token);

                // 回到待机状态
                attackState = AttackState.Idle;
                stateHandler.State = PlayerAnimatorState.Idle;
            }
            catch (OperationCanceledException)
            {
                // 技能被中断（例如切换武器或重新攻击）
                Debug.Log("[SkillAttack] Canceled");
            }
        }
        #endregion

        #region Parry / Dodge / Damage

        //当玩家按下招架键时由输入系统或角色控制器调用
        public bool TryParry()
        {
            if (Time.time - lastParryTime < parryCooldown) return false;
            lastParryTime = Time.time;
            isParrying = true;
            ParryWindowAsync().Forget();
            return true;
        }

        private async UniTaskVoid ParryWindowAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(parryWindow));
            isParrying = false;
        }

        //当敌人攻击命中玩家时判定是否招架成功
        public bool TryHandleIncomingAttackAsParry(GameObject attacker)
        {
            if (isParrying)
            {
                PropManager.AddSwordPoint(2);
                UIMessageSystem.Instance.AddMessage("招架成功，剑气+2");
                OnParrySuccess?.Invoke(attacker);
                OnParrySuccessEffect(attacker).Forget();
 
                isParrying = false;
                if (this == null || !isActiveAndEnabled)
                    return true;
                _ = SwordSoulManager.Instance.TriggerOnParryAsync(gameObject, attacker);
                return true;
            }
            return false;
        }

        public async UniTaskVoid OnParrySuccessEffect(GameObject attacker)
        {
            // 特效
            EffectManager.Instance.PlayEffect("Parry", attacker.transform.position + Vector3.up);
            // 镜头特写
            CameraManager.Instance.PlayParryCamera().Forget();
            // 短暂停顿（时间缩放）
            Time.timeScale = 0.1f;
            await UniTask.Delay(1300, ignoreTimeScale: true);
            Time.timeScale = 1f;
        }

        //当玩家按下闪避键时由输入系统调用
        public async UniTaskVoid TryDodgeAsync(float invulDuration = 0.35f)
        {
            // 防止重复闪避
            if (isInvulnerable) return;
            isInvulnerable = true;

            // 状态切换
            stateHandler.State = PlayerAnimatorState.Dodge;

            // 完美闪避判定
            bool isPerfect = CheckPerfectDodge();
            if (isPerfect)
            {
                Debug.Log("Perfect Dodge!");
                PropManager.AddSwordPoint(1);
                UIMessageSystem.Instance.AddMessage("极限闪避成功，剑气+1");
                OnPerfectDodge?.Invoke();
                OnPerfectDodgeEffect().Forget();
                if (this == null || !isActiveAndEnabled)
                    return;
                _ = SwordSoulManager.Instance.TriggerOnDodgeAsync(gameObject);
            }

            // 执行闪避位移
            await PerformDodgeMotionAsync();

            // 等待无敌时间结束
            await UniTask.Delay(TimeSpan.FromSeconds(invulDuration));
            isInvulnerable = false;

            // 动作结束 → 回Idle
            stateHandler.State = PlayerAnimatorState.Idle;
        }

        private async UniTask PerformDodgeMotionAsync()
        {
            float elapsed = 0f;

            // 获取当前朝向 + 闪避方向（默认朝右后方）
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            Vector3 dodgeDir = (-forward + right * 0.5f).normalized;

            // 闪避主循环
            while (elapsed < dodgeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dodgeDuration);
                float curveT = dodgeCurve.Evaluate(t);

                // 水平位移
                float horizontalSpeed = dodgeDistance * curveT / dodgeDuration;
                Vector3 move = horizontalSpeed * Time.deltaTime * dodgeDir;

                // 高度抛物线
                float heightOffset = Mathf.Sin(t * Mathf.PI) * dodgeHeight;
                move.y = heightOffset * Time.deltaTime * 10f;

                controller.Move(move);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // 轻微校正落地
            controller.Move(Vector3.down * 0.1f);
        }

        // 敌人每次即将攻击时调用
        public void NotifyIncomingAttack(GameObject attacker, float timeToHit)
        {
            lastIncomingAttackTime = Time.time;
            lastAttackETA = timeToHit;

            //播放攻击预警
            EffectManager.Instance.PlayEffectFollow("AttackWarn", attacker.transform, Vector3.up, lifeTime: 1f);
        }

        private bool CheckPerfectDodge()
        {
            // 若玩家最近一次被通知的攻击在 perfectDodgeWindow 时间内即将命中，则视为极限闪避
            if (Time.time - lastIncomingAttackTime < lastAttackETA &&
                lastAttackETA < perfectDodgeWindow)
            {
                return true;
            }
            return false;
        }

        public async UniTaskVoid OnPerfectDodgeEffect()
        {
            CameraManager.Instance.PlayDodgeCamera().Forget();
            Time.timeScale = 0.3f;
            await UniTask.Delay(2300, ignoreTimeScale: true);
            Time.timeScale = 1f;
        }

        public async UniTaskVoid ApplyHit(float damageAmount, int attackerId)
        {
            if (isInvulnerable) return;

            var prop = PropManager.Prop;
            var lastHp = prop.HP;
            var intDamage = (int)damageAmount;
            prop.HP -= intDamage;
            EventQueueSystem.QueueEvent(new PopupTextEvent(transform, intDamage,Color.blue));
            EventQueueSystem.QueueEvent(new PlayerHpChangeEvent(lastHp, prop.HP, prop.MaxHP));
            if (prop.HP <= 0)
            {
                Death().Forget();
                return;
            }

            stateHandler.State = PlayerAnimatorState.Hurt;
            OnHurt?.Invoke(damageAmount, attackerId);

            isInvulnerable = true;
            await this.Delay(hurtInvulDuration);
            isInvulnerable = false;
        }

        private async UniTaskVoid Death()
        {
            stateHandler.State = PlayerAnimatorState.Dead;
            EventQueueSystem.RemoveListener<SendDamageEvent>(DamageEventHandler);
            EffectManager.Instance.PlayEffect("Death", transform.position + Vector3.up);
            GameManager.stageManager.RemoveOnePlayer(InsId);

            await UniTask.Delay(TimeSpan.FromSeconds(1));
            EventQueueSystem.QueueEvent(new StageStatesEvent(StageStates.GameOver));
        }

        private void DamageEventHandler(SendDamageEvent e)
        {
            if (e.damageActonType == DamageActonType.Trigger && e.enterObj.GetInstanceID() != InsId) return;
            if (e.damageActonType == DamageActonType.PointTo && e.targetId != InsId) return;
            if (e.damageActonType == DamageActonType.Range && !e.rangeObjs.Contains(gameObject)) return;
            ApplyHit(e.damage, e.sourceId).Forget();
        }

        private void OnDestroy()
        {
            EventQueueSystem.RemoveListener<SendDamageEvent>(DamageEventHandler);
        }
        #endregion
    }
}
