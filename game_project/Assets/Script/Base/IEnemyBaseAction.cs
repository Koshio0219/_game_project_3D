using Game.Data;

namespace Game.Base
{
    interface IEnemyBaseAction : IDamageable
    {
        EnemyUnitData EnemyUnitData { get; }
        EnemyState EnemyState { get; }
        AttackState EnemyAttackState { get; }

        void Born(EnemyUnitData data);
        void Dead();
        void Attack(int targetId, float damage);
        //void Hit(int sourceId, float damage);
        void Move();
        void Idle();
    }
}
