namespace Game.Framework
{
    public class EventDelegate : Singleton<EventDelegate>
    {
        public EventQueueSystem.EventDelegate<TestGameEvent> TestGameEventHandler { get; set; }
        //something else global event delegates...
    }
}