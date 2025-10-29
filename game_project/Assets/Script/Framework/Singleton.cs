namespace Game.Framework
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T instance = null;

        private static readonly object locker = new();

        public static T Instance
        {
            get
            {
                lock (locker)
                {
                    instance ??= new T();
                    return instance;
                }
            }
        }
    }
}
