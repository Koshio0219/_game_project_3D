using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Framework;
using Game.Player;
using UnityEngine;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "Parry_Damage_Soul", menuName = "Game/Soul/ParryDamageSoul")]
    public class ParryDamageSoul : SwordSoul
    {
        public int baseDamage = 200;

        public override async UniTask ApplyEffectAsync(GameObject owner, GameObject context)
        {
            // context 可以是被招架的敌人 GameObject
            if (owner!= null && context != null)
            {
                if (context.transform.root.TryGetComponent<IEnemyBaseAction>(out var enemy))
                {
                    EventQueueSystem.QueueEvent(new SendDamageEvent(owner.transform.root.GetInstanceID(), enemy.EnemyUnitData.InsId, baseDamage));
                }
            }

            await UniTask.Delay(200);
        }
    }
}
