using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Data;
using Game.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Game.Player
{
    public enum AttackState
    {
        Idle,
        Attacking,
        ComboQueued,
        Cooldown
    }

    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerStateHandler))]
    public class PlayerAttackCtrl : MonoBehaviour, IDamageable
    {
        private int? _insId;
        public int InsId => _insId ??= gameObject.GetInstanceID();

        public PlayerPropManager PropManager { get; private set; }

        [Header("Weapon")]
        public SerializedDictionary<WeaponHandType, Transform> weaponTransform;
        private Weapon currentWeapon;
        private WeaponHitbox currentWeaponHitbox;

        private PlayerInputHandler input;
        private PlayerStateHandler stateHandler;

        [Header("Attack Timings")]
        public float normalAttackDelay = 0.1f;
        public float normalHitWindow = 0.18f;
        public float skillHitRadius = 3f;
        public float skillHitDelay = 0.15f;
        public float skillHitWindow = 0.35f;

        [Header("Parry / Dodge")]
        public float parryWindow = 0.25f;
        public float parryCooldown = 1f;
        private float lastParryTime = -10f;
        private bool isParrying = false;
        private bool isInvulnerable = false;

        public event Action<GameObject> OnParrySuccess;
        public event Action<float, int> OnHurt;

        private AttackState attackState = AttackState.Idle;
        private CancellationTokenSource attackCTS;

        private void Awake()
        {
            input = GetComponent<PlayerInputHandler>();
            stateHandler = GetComponent<PlayerStateHandler>();
            EventQueueSystem.AddListener<SendDamageEvent>(DamageEventHandler);
        }

        private void Start()
        {
            GameManager.stageManager.AddOnePlayer(InsId, gameObject);
            EnterLevel();
        }

        public void EnterLevel()
        {
            PropManager = new PlayerPropManager(new PlayerData(GameManager.Instance.gameData.playerConfig.maxSwordPoint));
            EquipWeapon(GameManager.Instance.gameData.playerConfig.initWeaponId);
            EventQueueSystem.QueueEvent(new PlayerEnterLevelEvent(PropManager.Prop));
        }

        public void EquipWeapon(int weaponId)
        {
            if (currentWeapon != null) return;
            var data = GameManager.Instance.gameData.playerWeaponDatas[weaponId];

            currentWeapon = GameObjectPool.Instance.GetObj(data.prefab, weaponTransform[data.handType]).GetComponent<Weapon>();
            currentWeapon.transform.ResetLocal();
            currentWeapon.InitWeapon(weaponId);
            PropManager.AddProp(currentWeapon.AddProp);

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

            if (input.AttackPressed)
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
            attackCTS?.Cancel();
            attackCTS = new CancellationTokenSource();
            stateHandler.State = PlayerAnimatorState.Attack;

            await UniTask.Delay(TimeSpan.FromSeconds(skillHitDelay), cancellationToken: attackCTS.Token);

            var hits = Physics.OverlapSphere(transform.position, skillHitRadius);
            var list = new List<GameObject>();
            foreach (var c in hits)
            {
                if (c.gameObject == gameObject) continue;
                var dmgable = c.GetComponentInParent<IDamageable>();
                if (dmgable != null)
                {
                    list.Add(c.gameObject);
                }
            }
            float dmg = PropManager.CalSkillAttackDamaage();
            EventQueueSystem.QueueEvent(new SendDamageEvent(InsId, list, dmg));
            await UniTask.Delay(TimeSpan.FromSeconds(skillHitWindow), cancellationToken: attackCTS.Token);
        }
        #endregion

        #region Parry / Dodge / Damage
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

        public bool TryHandleIncomingAttackAsParry(GameObject attacker)
        {
            if (isParrying)
            {
                OnParrySuccess?.Invoke(attacker);
                isParrying = false;
                return true;
            }
            return false;
        }

        public async UniTaskVoid TryDodgeAsync(float invulDuration = 0.35f)
        {
            if (isInvulnerable) return;
            isInvulnerable = true;
            stateHandler.State = PlayerAnimatorState.Dodge;
            await UniTask.Delay(TimeSpan.FromSeconds(invulDuration));
            isInvulnerable = false;
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Debug.Log("玩家撞到了：" + hit.gameObject.name);
        }

        public void ApplyHit(float damageAmount, int attackerId)
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
        }

        private async UniTaskVoid Death()
        {
            stateHandler.State = PlayerAnimatorState.Dead;
            EventQueueSystem.RemoveListener<SendDamageEvent>(DamageEventHandler);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            EventQueueSystem.QueueEvent(new StageStatesEvent(StageStates.GameOver));
        }

        private void DamageEventHandler(SendDamageEvent e)
        {
            if (e.damageActonType == DamageActonType.Trigger && e.enterObj.GetInstanceID() != InsId) return;
            if (e.damageActonType == DamageActonType.PointTo && e.targetId != InsId) return;
            if (e.damageActonType == DamageActonType.Range && !e.rangeObjs.Contains(gameObject)) return;
            ApplyHit(e.damage, e.sourceId);
        }

        #endregion
    }
}
