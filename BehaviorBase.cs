using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BetterDriver
{
    //base class.
    public abstract class Behavior : ISchedulable, IObserver, IUtilized
    {
        public event SchedulableHandler Completed;
        public NodeStatus Status { get; protected set; }
        public float Utility { get; protected set; }

        public void OnComplete(ISchedulable sender) { Completed?.Invoke(sender); }
        public virtual void Clear() { Status = NodeStatus.SUSPENDED; }
        public virtual void Abort() { Status = NodeStatus.ABORTED; }
        public virtual void CalculateUtility() { }

        public abstract void Step(float dt);
        public abstract void Enter();
        public abstract void Init();
    }

    // leaf nodes.
    public abstract class Action : Behavior
    {
        protected IScheduler scheduler;
        public Action(IScheduler s) => scheduler = s;
        public override void Enter() { Clear(); scheduler.PostSchedule(this); }
        public override void Init() { }
    }
    public abstract class Condition : Behavior
    {
        protected IScheduler scheduler;
        public Condition(IScheduler s) => scheduler = s;
        public override void Enter() { Clear(); scheduler.PostSchedule(this); }
        public override void Init() { }
    }
}
