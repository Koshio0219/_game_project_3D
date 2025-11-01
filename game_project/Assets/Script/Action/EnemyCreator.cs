using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Data;
using Game.Framework;
using Game.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Game.Action
{
    public class EnemyCreator : MonoBehaviour, IInit
    {
        private EnemyCreateConfig enemyCreateConfig;

        private readonly List<IEnemyBaseAction> insEnemy = new();

        private UnityAction buildAction = null;

#if DEBUG_MODE
        private void Start()
        {
            buildAction.Invoke();
        }
#endif

        private void Awake()
        {
            Init();
            EventQueueSystem.AddListener<StageStatesEvent>(StageStatesHandler);
        }

        private void OnDestroy()
        {
            buildAction = null;
            insEnemy.Clear();
            EventQueueSystem.RemoveListener<StageStatesEvent>(StageStatesHandler);
        }

        private void StageStatesHandler(StageStatesEvent e)
        {
            if (e.to != StageStates.EnemyBuildStart) return;
            if (enemyCreateConfig == null)
            {
                Debug.LogError("enemyCreateConfig is null,please check");
                EventQueueSystem.QueueEvent(new StageStatesEvent(StageStates.EnemyBuildEnd));
                return;
            }

            buildAction.Invoke();
        }

        public void Init()
        {
            //enemyCreateConfig = GameData.Instance.EnemyCreateConfig;

            //buildAction = UniTask.UnityAction(async (_) =>
            //{
            //    insEnemy.Clear();
            //    Debug.Log("enemy build start!");
            //    if (enemyCreateConfig == null) return;
            //    var levelIdx = GameManager.Instance.LevelIdx;
            //    var data = enemyCreateConfig.levelEnemyData[levelIdx];
            //    foreach (var one in data.enemies)
            //    {
            //        var createData = enemyCreateConfig.MapEnemyTypeIDToData[one.typeID];
            //        var ins = await AssetLoader.Instance.Load<Enemy>
            //        (createData.assetReference, this.GetCancellationTokenOnDestroy(), false);
            //        ins.transform.position = one.randomOnBlock ? 
            //        GameManager.stageManager.SelecteOneBlockPoint().position : one.pos;
            //        //if (one.randomOnBlock)
            //        //{
            //        //    var createPoint = GameManager.stageManager.SelecteOneBlockPoint();            
            //        //    ins.SetBlockPoint(createPoint.parent);
            //        //    ins.transform.position = createPoint.position;
            //        //}
            //        //else
            //        //{
            //        //    ins.transform.position = one.pos;
            //        //}
            //        //face to left
            //        ins.transform.forward = Vector3.left;
            //        ins.Born(createData.unitData);
            //        insEnemy.Add(ins);
            //        await UniTask.DelayFrame(1);
            //    }
            //    Debug.Log("enemy build end!");
            //    EventQueueSystem.QueueEvent(new StageStatesEvent(StageStates.EnemyBuildEnd));
            //}, this.GetCancellationTokenOnDestroy());
        }
    }
}