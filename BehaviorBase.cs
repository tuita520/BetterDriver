using System;
using System.Collections.Generic;

namespace BetterDriver
{
    //base class.
    public abstract class Behavior : ISchedulable
    {
        protected IScheduler scheduler;
        public NodeStatus Status { get; protected set; }
        public Guid ID { get; }

        public Behavior(IScheduler s)
        {
            ID = Guid.NewGuid();
            scheduler = s;
        }
        public virtual void Clear() { Status = NodeStatus.SUSPENDED; }
        public virtual void Abort() { Status = NodeStatus.ABORTED; }

        public abstract void Step(float dt);
        public abstract void Enter();
    }

    // leaf nodes.
    public abstract class Action : Behavior
    {
        public Action(IScheduler s) : base(s) { }
        public override void Enter() { Clear(); scheduler.PostSchedule(this); }
    }
    public abstract class Condition : Behavior
    {
        public Condition(IScheduler s) : base(s) { }
        public override void Enter() { Clear(); scheduler.PostSchedule(this); }

    }

    //branch nodes.
    public abstract class Decorator : Behavior
    {
        protected Behavior Child;

        public Decorator(IScheduler s) : base(s)
        {
        }

        public void SetChild(Behavior child) { Child = child; scheduler.SubscribeChildComplete(child, OnChildCompleted); }
        public override void Abort() { base.Abort(); Child.Abort(); }
        public override void Step(float dt) { Status = NodeStatus.SUSPENDED; }
        public override void Enter()
        {
            if (Child == null) SetChild(new FakeSuccessAction(scheduler));
            Clear();
            Child.Enter();
        }
        public abstract void OnChildCompleted(ISchedulable sender);
    }

    public abstract class Composite : Behavior
    {
        protected int CurrentIndex;
        protected List<Behavior> Children = new List<Behavior>();

        public Composite(IScheduler s) : base(s)
        {
        }

        public void AddChild(Behavior child) { Children.Add(child); scheduler.SubscribeChildComplete(child, OnChildCompleted); }
        public void RemoveChild(Behavior child) { Children.Remove(child); scheduler.UnsubscribeChildComplete(child); }
        public void ClearChildren()
        {
            foreach (var child in Children)
            {
                scheduler.UnsubscribeChildComplete(child);
            }
            Children.Clear();
        }
        public override void Abort()
        {
            base.Abort();
            foreach (var child in Children)
            {
                child.Abort();
            }
        }
        public override void Clear()
        {
            CurrentIndex = 0;
            base.Clear();
        }
        public override void Step(float dt) { Status = NodeStatus.SUSPENDED; }
        public override void Enter()
        {
            if (Children.Count == 0) Children.Add(new FakeSuccessAction(scheduler));
            Clear();
            var child = Children[CurrentIndex];
            child.Enter();
        }
        public abstract void OnChildCompleted(ISchedulable sender);
    }
}
