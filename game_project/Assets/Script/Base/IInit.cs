namespace Game.Base
{
    public interface IInit
    {
        void Init();
    }

    public interface IInit<T>
    {
        void Init(T data);
    }
}
