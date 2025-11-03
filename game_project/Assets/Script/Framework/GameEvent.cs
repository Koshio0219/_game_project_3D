//Game中の全てのEventはこのScriptに書く、そして確認できる
//EventはGameEventというClassを継承する必要がある
//using Game.Manager;
using Game.Player;
using System.Collections.Generic;
using UnityEngine;
using Game.Base;

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

    public class PlayerHpChangeEvent : GameEvent
    {
        public float lastHp;
        public float nowHp;
        public float maxHp;
        public PlayerHpChangeEvent(float lastHp, float nowHp, float maxHp)
        {
            this.lastHp = lastHp;
            this.nowHp = nowHp;
            this.maxHp = maxHp;
        }
    }

    public class UpdateNavMeshEvent : GameEvent { }

    public class SwordPointChangeEvent: GameEvent
    {
        public int lastP;
        public int nowP;
        public int maxP;

        public SwordPointChangeEvent(int lastP, int nowP, int maxP)
        {
            this.lastP = lastP;
            this.nowP = nowP;
            this.maxP = maxP;
        }
    }

    public class PlayerEnterLevelEvent : GameEvent 
    {
        public PlayerData playerData;
        public PlayerEnterLevelEvent(PlayerData playerData)
        {
            this.playerData = playerData;
        }
    }

    public class PlayerExitLevelEvent : GameEvent { }
   

    public class  PlayerDeadEvent : GameEvent
    {
        
    }

    public class  PopupTextEvent:GameEvent
    {
        public Transform target;
        public int num;

        public PopupTextEvent(Transform _target,int _num)
        {
            target = _target;
            num = _num;
        }
    }

    public class StageStatesEvent : GameEvent
    {
        public StageStates to;
        public StageStatesEvent(StageStates stageStates)
        {
            to = stageStates;
        }
    }

    public class PointChangeEvent : GameEvent
    {
        public int lastPoint;
        public int nowPoint;

        public PointChangeEvent(int lastPoint, int nowPoint)
        {
            this.lastPoint = lastPoint;
            this.nowPoint = nowPoint;
        }
    }
}