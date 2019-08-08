
namespace BetterDriver
{
    public delegate void SchedulableHandler(ISchedulable sender);
    public interface IBlackBoard
    {
        T Get<T>(string key);
        void Post<T>(string key, T value);
    }

    public interface IObserver
    {
        event SchedulableHandler Completed;
        void OnComplete(ISchedulable sender);
    }
    public interface ISchedulable
    {
        NodeStatus Status { get; }
        void Enter();
        void Step(float dt);
        void Init(); // this is the method to setup callbacks.
    }
    public interface IScheduler
    {
        void PostSchedule(ISchedulable schedule);
    }
    public interface IUtilized
    {
        float Utility { get; }
        void CalculateUtility();
    }
}
