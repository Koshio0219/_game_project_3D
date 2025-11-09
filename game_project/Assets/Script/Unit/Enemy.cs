using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Data;
using Game.Framework;
using KanKikuchi.AudioManager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Unit
{
    public class Enemy : MonoBehaviour, IEnemyBaseAction, IInit
    {
        private EnemyUnitData enemyUnitData;
        public EnemyUnitData EnemyUnitData => enemyUnitData;

        private EnemyState state;
        private EnemyState State { get => state; set => state = value; }
        public EnemyState EnemyState => State;

        protected readonly Dictionary<EnemyState, UnityAction> mapStateToAction = new(5);

        private AttackState attackState;
        private AttackState AttackState { get => attackState; set => attackState = value; }
        public AttackState EnemyAttackState => AttackState;

        private float maxHp;
        public virtual float MaxHp
        {
            get
            {
                return maxHp;
            }
            set
            {
                maxHp = value;
            }
        }

        private float hp;
        public virtual float Hp
        {
            get
            {
                return hp;
            }
            set
            {
                if (hp == value) return;
                var last = hp;
                hp = value;

                if (hp > MaxHp) hp = MaxHp;
                if (hp <= 0) Dead();

                EventQueueSystem.QueueEvent(new EnemyHpChangeEvent(EnemyUnitData.InsId, last, hp));
            }
        }

        private float atk;
        public virtual float Atk
        {
            get
            {
                return atk;
            }
            set
            {
                atk = value;
            }
        }

        [SerializeField] protected Animator animator;
        [SerializeField] protected BehaviorTree behaviorTree;

        private GameObject player;

        //行为树等外部调用
        public virtual async void Attack(int targetId, float damage)
        {
            ChangeState(EnemyState.Attack);

            if (player == null)
            {
                Debug.LogWarning("Player not found when enemy attacks.");
                return;
            }
            float timeToHit = 0.3f; //攻击前摇
            // 检查玩家是否存在Parry系统组件
            if (!player.TryGetComponent<Player.PlayerAttackCtrl>(out var attackSystem))
                return;
            //通知玩家有攻击即将命中
            attackSystem.NotifyIncomingAttack(gameObject, timeToHit);
            await UniTask.Delay(TimeSpan.FromSeconds(timeToHit));

            // 如果敌人已死亡、中断攻击、被击退等，可提前终止
            if (this == null || !isActiveAndEnabled)
                return;

            // 限定只有近战或Boss敌人才会触发招架逻辑
            if (enemyUnitData.attackType == EnemyAttackType.Close ||
                enemyUnitData.attackType == EnemyAttackType.Boss)
            {
                bool parried = attackSystem.TryHandleIncomingAttackAsParry(gameObject);
                if (parried)
                {
                    Debug.Log($"Enemy {name}'s attack was parried by player!");
                    return; // 攻击被招架则不继续执行伤害逻辑
                }
            }

            //招架失败，传递伤害
            Debug.Log($"Attacking! targetId:{targetId},damage:{damage}");
            EventQueueSystem.QueueEvent(new SendDamageEvent(enemyUnitData.InsId, targetId, damage + Atk));
        }

        public virtual void Born(EnemyUnitData data)
        {
            Init();
            data.Init();
            enemyUnitData = data;
            InitBaseProp(data.prop);

            var list = new List<Transform>();
            foreach (var item in GameManager.stageManager.GetAllPlayer())
                list.Add(item.transform);

            //one player 
            player = list[0].gameObject;

            InitBehaviorTree(list);
            GameManager.stageManager.AddOneEnemy(data.InsId, this);
            ChangeState(EnemyState.Idle);

            EventQueueSystem.QueueEvent(new InitEnemyHpEvent(data.InsId, MaxHp));
            EventQueueSystem.AddListener<SendDamageEvent>(DamageEventHandler);
        }

        public virtual async void Dead()
        {
            CalDeadPoint();
            EventQueueSystem.RemoveListener<SendDamageEvent>(DamageEventHandler);
            ChangeState(EnemyState.Dead);
            //dead animation time delay
            await UniTask.Delay(1000);


            if (GameManager.stageManager.StageState == StageStates.BattleClear) return;
            //EffectManager.Instance.Play(EffectManager.EffectID.EnemyDead, this.transform.position);
            SEManager.Instance.Play(SEPath.ENEMY_DEAD);
            GameManager.stageManager.RemoveOneEnemy(enemyUnitData.InsId);

        }

        protected virtual void DamageEventHandler(SendDamageEvent e)
        {
            if (e.damageActonType == DamageActonType.Trigger && e.enterObj.GetInstanceID() != gameObject.GetInstanceID()) return;
            if (e.damageActonType == DamageActonType.PointTo && e.targetId != enemyUnitData.InsId) return;
            if (e.damageActonType == DamageActonType.Range && !e.rangeObjs.Contains(gameObject)) return;
            Hit(e.sourceId, e.damage);
        }

        public virtual void Hit(int sourceId, float damage)
        {
            if (sourceId == enemyUnitData.InsId) return;
            if (GameManager.stageManager.IsFriend(sourceId, enemyUnitData.InsId)) return;

            EventQueueSystem.QueueEvent(new PopupTextEvent(transform, (int)damage, Color.white));
            Hp -= damage;
            Debug.Log($"enemy id :{enemyUnitData.InsId},name:{gameObject.name} had receive damage:{damage},current hp :{Hp}");
            ChangeState(EnemyState.Hit);
        }

        public virtual void Move()
        {
            ChangeState(EnemyState.Moving);
        }

        protected virtual void InitBaseProp(EnemyBaseProp baseProp)
        {
            maxHp = baseProp.maxHp;
            hp = baseProp.Hp;
            atk = baseProp.attack;
        }

        protected virtual void ChangeState(EnemyState toState)
        {
            if (State == toState) return;
            State = toState;
            if (mapStateToAction.Count == 0) return;
            mapStateToAction[toState].Invoke();
        }

        public virtual void ChangeAttackState(AttackState toState)
        {
            if (AttackState == toState) return;
            AttackState = toState;
            OnChangeAttackState(toState);
        }

        protected virtual void OnChangeIdle() { }
        protected virtual void OnChangeDead() { }
        protected virtual void OnChangeHit() { }
        protected virtual void OnChangeMove() { }
        protected virtual void OnChangeAttack() { }
        protected virtual void OnChangeAttackState(AttackState attackState) { }

        public void Init()
        {
            if (mapStateToAction.Count > 0) return;
            mapStateToAction.Add(EnemyState.Idle, OnChangeIdle);
            mapStateToAction.Add(EnemyState.Dead, OnChangeDead);
            mapStateToAction.Add(EnemyState.Hit, OnChangeHit);
            mapStateToAction.Add(EnemyState.Moving, OnChangeMove);
            mapStateToAction.Add(EnemyState.Attack, OnChangeAttack);
        }

        private HashSet<int> _enteredColliders = new();

        void OnTriggerEnter(Collider other)
        {
            //忽略玩家的子物体（如手持武器）
            if (!other.TryGetComponent<IDamageable>(out _)) return;
            var up = other.transform.root;
            // 去重：只在第一次进入时执行逻辑
            var instanceId = up.GetInstanceID();
            if (_enteredColliders.Contains(instanceId)) return;
            _enteredColliders.Add(instanceId);

            Debug.Log($"enemy name:{gameObject.name} had OnTriggerEnter,target name:{up.name}");
            var pId = GameManager.stageManager.MatchPlayerId(up.gameObject);
            if (pId == -1) return;
            EventQueueSystem.QueueEvent(new SendDamageEvent(enemyUnitData.InsId, up.gameObject, Atk * 0.5f));
        }

        void OnTriggerExit(Collider other)
        {
            var up = other.transform.root;
            var instanceId = up.GetInstanceID();
            if (!_enteredColliders.Contains(instanceId)) return;
            _enteredColliders.Remove(instanceId);

            Debug.Log($"enemy name:{gameObject.name} had OnTriggerExit,target name:{up.name}");
        }

        protected virtual void InitBehaviorTree(List<Transform> list)
        {
            behaviorTree.SetProp("TargetList", list);
            behaviorTree.SetProp("Self", gameObject);
            //test null
            //behaviorTree.SetProp("TargetList156", 12);
        }

        public virtual void Idle()
        {
            ChangeState(EnemyState.Idle);
        }

        protected virtual void CalDeadPoint()
        {
            switch (EnemyUnitData.raceType)
            {
                case EnemyRaceType.Slime:
                    GameManager.pointManager.AddPoint(GetPointItem.KillSlime, EnemyUnitData.prop.killPoint);
                    break;
                case EnemyRaceType.Guard:
                    GameManager.pointManager.AddPoint(GetPointItem.KillGuard, EnemyUnitData.prop.killPoint);
                    break;
                case EnemyRaceType.Ghost:
                    GameManager.pointManager.AddPoint(GetPointItem.KillGhost, EnemyUnitData.prop.killPoint);
                    break;
            }
        }

        protected virtual void OnDestroy()
        {
            EventQueueSystem.RemoveListener<SendDamageEvent>(DamageEventHandler);
        }
    }
}