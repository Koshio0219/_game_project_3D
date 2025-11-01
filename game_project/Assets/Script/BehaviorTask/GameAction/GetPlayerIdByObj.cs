using Tasks = BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Game.Framework;

namespace Game.BehaviorTask
{
    public class GetPlayerIdByObj : Tasks.Action
    {
        [RequiredField]
        public SharedGameObject target;
        public SharedInt playerId = -1;

        public override TaskStatus OnUpdate()
        {
            if (target.Value == null) return TaskStatus.Failure;

            //var up = target.Value.transform.GetRootParent();
            //var pId = GameManager.stageManager.MatchPlayerId(up.gameObject);
            //if (pId == -1) return TaskStatus.Failure;
            //playerId.Value = pId;

            return TaskStatus.Success;
        }
    }
}