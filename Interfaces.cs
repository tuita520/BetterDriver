using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public interface IComposable
    {
        void AddChild(Behavior c);
        void RemoveChild(Behavior c);
        void ClearChildren();
    }
    public interface IUpdatable
    {
        BehaviorStatus Status { get; }
        void Step(float dt);
        void Abort();
    }
    public interface ISchedulable
    {
        Guid ID { get; }
        void Setup(IScheduler scheduler);
        void OnCompleted(IScheduler scheduler, BehaviorStatus status);
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
