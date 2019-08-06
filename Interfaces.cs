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
        ref readonly NodeStatus Status { get; }
        void Init(IScheduler scheduler);
        void OnCompleted(IScheduler scheduler, NodeStatus status);
        void Step(IBlackBoard bb, float dt);
    }
    public interface IScheduler
    {
        void PostSchedule(ISchedulable schedule);
        void PostCallBack(ISchedulable schedule, Action<IScheduler, NodeStatus> cb);
        void Terminate(ISchedulable schedule, NodeStatus status);
    }
    public interface IBehaviorTreeBuilder
    {
        BehaviorTree Build();
    }
    public interface IStackable<T>
    {
        T Parent { get; }
        List<T> Children { get; }
    }
}
