using Game.Data;

namespace Game.Base
{
    public interface IFireOption
    {
        void FireBegin(int creatorId = 0, AttackState attackState = AttackState.Normal);

        void FireShut();
    }
}

