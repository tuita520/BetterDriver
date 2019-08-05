using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public interface IBlackBoard
    {
        T Get<T>(string key);
        void Post<T>(string key, T value);
    }
    public interface ISchedulable
    {
        Guid ID { get; }
        BehaviorStatus Status { get; }
        void Init(IScheduler scheduler);
        void OnCompleted(IScheduler scheduler, BehaviorStatus status);
        void Step(IBlackBoard bb, float dt);
        void Abort();
        void Clear();
    }
    public interface IScheduler
    {
        void PostSchedule(ISchedulable schedule);
        void PostCallBack(ISchedulable schedule, Action<IScheduler, BehaviorStatus> cb);
        void Terminate(ISchedulable schedule, BehaviorStatus status);
    }
    public interface IStackable<T>
    {
        T Parent { get; }
        List<T> Children { get; }
    }
}
