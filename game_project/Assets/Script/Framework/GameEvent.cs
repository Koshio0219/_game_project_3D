//Game中の全てのEventはこのScriptに書く、そして確認できる
//EventはGameEventというClassを継承する必要がある
//using Game.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Framework
{
    interface IEventListenerAction
    {
        void AddListeners();
        void RemoveListeners();
    }

    public class TestGameEvent : GameEvent
    {
        public string test;

        public TestGameEvent(string test)
        {
            this.test = test;
        }
    }

    public class SceneLoadStartEvent : GameEvent { }

    public class SceneLoadProgressChangeEvent : GameEvent
    {
        public float progress;

        public SceneLoadProgressChangeEvent(float progress)
        {
            this.progress = progress;
        }
    }

    public class SceneLoadFinishedEvent : GameEvent { }

    public class BackgroundScrollerEvent : GameEvent 
    {
        public float dis;

        public BackgroundScrollerEvent(float dis)
        {
            this.dis = dis;
        }
    }

    public enum DamageActonType
    {
        Trigger,
        PointTo,
        Range
    }

    public class SendDamageEvent : GameEvent
    {
        public DamageActonType damageActonType;
        public int sourceId;
        //type PointTo
        public int targetId;
        //type Trigger
        public GameObject enterObj;
        //type Range
        public List<GameObject> rangeObjs; 
        public float damage;

        //type trigger
        public SendDamageEvent(int sourceId, GameObject enterObj, float damage)
        {
            this.sourceId = sourceId;
            this.enterObj = enterObj;
            this.damage = damage;
            damageActonType = DamageActonType.Trigger;
        }

        //type pointto
        public SendDamageEvent(int sourceId, int targetId, float damage)
        {
            this.sourceId = sourceId;
            this.targetId = targetId;
            this.damage = damage;
            damageActonType = DamageActonType.PointTo;
        }

        //type range
        public SendDamageEvent(int sourceId, List<GameObject> rangeObjs, float damage)
        {
            this.sourceId = sourceId;
            this.rangeObjs = rangeObjs;
            this.damage = damage;
            damageActonType = DamageActonType.Range;
        }
    }

    public class InitEnemyHpEvent : GameEvent
    {
        public int enemyId;
        public float hp;

        public InitEnemyHpEvent(int enemyId, float hp)
        {
            this.enemyId = enemyId;
            this.hp = hp;
        }
    }

    public class EnemyHpChangeEvent : GameEvent
    {
        public int enemyId;
        public float lastHp;
        public float nowHp;
        public EnemyHpChangeEvent(int enemyId, float lastHp, float nowHp)
        {
            this.enemyId = enemyId;
            this.lastHp = lastHp;
            this.nowHp = nowHp;
        }
    }

    public enum PlayerType
    {
        Swordsman,
        Witch
    }

    public class PlayerHpChangeEvent : GameEvent
    {
        public PlayerType playerType;
        public float lastHp;
        public float nowHp;
        public PlayerHpChangeEvent(PlayerType playerType, float lastHp, float nowHp)
        {
            this.playerType = playerType;
            this.lastHp = lastHp;
            this.nowHp = nowHp;
        }
    }

    public class UpdateNavMeshEvent : GameEvent { }

    public class PointChangeEvent: GameEvent
    {
        public int lastP;
        public int nowP;
        public bool reachGoal;

        public PointChangeEvent(int lastP, int nowP, bool reachGoal)
        {
            this.lastP = lastP;
            this.nowP = nowP;
            this.reachGoal = reachGoal;
        }
    }

    public class ReachPointEvent : GameEvent { }

    public class StageTimeUpEvent : GameEvent {
    
    }

    public class  PlayerDeadEvent : GameEvent
    {
        
    }

    public class  PopupTextEvent:GameEvent
    {
        public Transform target;
        public Color color;
        public int num;

        public PopupTextEvent(Transform _target,int _num,Color _color)
        {
            target = _target;
            num = _num;
            color = _color;
        }
    }

    public class EnterDeadZoneEvent : GameEvent
    {
        public int rootGoInsId;
        public EnterDeadZoneEvent(int rootGoInsId)
        {
            this.rootGoInsId = rootGoInsId;
        }
    }

    public class BlockDragStartEvent : GameEvent 
    {
        public Transform targetBlock;
        public BlockDragStartEvent(Transform transform)
        {
            targetBlock = transform;
        }
    }

    public class BlockDragEndEvent : GameEvent { }
}