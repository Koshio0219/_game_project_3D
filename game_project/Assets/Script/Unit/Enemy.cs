using Cysharp.Threading.Tasks;
using Game.Action;
using Game.Base;
using Game.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Game.Framework;
using System;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using System.Threading.Tasks;
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

        public virtual void Attack(int targetId, float damage)
        {
            ChangeState(EnemyState.Attack);
            Debug.Log($"Attacking! targetId:{targetId},damage:{damage}");
        }

        public virtual void Born(EnemyUnitData data)
        {
            Init();
            data.Init();
            enemyUnitData = data;
            InitBaseProp(data.prop);
            InitBehaviorTree();
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
            //SEManager.Instance.Play(SEPath.ENEMY_DEAD);
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

            EventQueueSystem.QueueEvent(new PopupTextEvent(transform, (int)damage, Color.blue));
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

        private void OnCollisionEnter(Collision collision)
        {
            var up = collision.transform.GetRootParent();
            Debug.Log($"enemy name:{gameObject.name} had OnCollisionEnter,target name:{up.name}");
            if (!up.TryGetComponent<IDamageable>(out _)) return;
            var pId = GameManager.stageManager.MatchPlayerId(up.gameObject);
            if (pId == -1) return;
            EventQueueSystem.QueueEvent(new SendDamageEvent(enemyUnitData.InsId, up.gameObject, Atk));
        }

        protected virtual void InitBehaviorTree() 
        {
            var list = new List<Transform>();
            foreach (var item in GameManager.stageManager.GetAllPlayer())
            {
                list.Add(item.transform);
            }

            behaviorTree.SetProp("TargetList", list);
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