using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public delegate void SchedulableHandler(ISchedulable sender);
    public interface IBlackBoard
    {
        T Get<T>(string key);
        void Post<T>(string key, T value);
    }

    public interface ISchedulable
    {
        Guid ID { get; }
        NodeStatus Status { get; }
        void Enter();
        void Step(float dt);
    }
    public interface IScheduler
    {
        void PostSchedule(ISchedulable schedule);
        void SubscribeChildComplete(ISchedulable child, SchedulableHandler cb);
        void UnsubscribeChildComplete(ISchedulable child);
        void OnChildComplete(ISchedulable sender);
    }
}
