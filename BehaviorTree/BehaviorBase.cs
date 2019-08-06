using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    //base class.
    public abstract class Behavior : ISchedulable, IObserver
    {
        protected NodeStatus status = NodeStatus.SUSPENDED;
        public ref readonly NodeStatus Status
        {
            get { return ref status; }
        }
        public Guid ID { get; }
        public Behavior() => ID = Guid.NewGuid();
        public virtual void Abort() { status = NodeStatus.ABORTED; }
        public void Clear() { status = NodeStatus.SUSPENDED; }
        public abstract void Step(IBlackBoard bb, float dt);
        public abstract void Enter(IScheduler scheduler);
        public abstract void OnCompleted(IScheduler scheduler, NodeStatus status);
    }

    // leaf nodes.
    public abstract class Action : Behavior
    {
        public override void Enter(IScheduler scheduler) { Clear(); }
        public override void OnCompleted(IScheduler scheduler, NodeStatus status) { }
    }
    public abstract class Condition : Behavior
    {
        public override void Enter(IScheduler scheduler) { Clear(); }
        public override void OnCompleted(IScheduler scheduler, NodeStatus status) { }

    }

    //branch nodes.
    public abstract class Decorator : Behavior
    {
        protected Behavior Child;
        public void SetChild(Behavior child) { Child = child; }
        public override void Abort() { base.Abort(); Child.Abort(); }
        public override void Step(IBlackBoard bb, float dt) { status = NodeStatus.SUSPENDED; }
        public override void Enter(IScheduler scheduler)
        {
            if (Child == null) SetChild(new FakeSuccessAction());
            Clear();
            scheduler.PostSchedule(Child);
            Child.Enter(scheduler);
        }
    }

    public abstract class Composite : Behavior
    {
        protected int CurrentIndex;
        protected List<Behavior> Children = new List<Behavior>();
        public void AddChild(Behavior child) { Children.Add(child); }
        public void RemoveChild(Behavior child) { Children.Remove(child); }
        public void ClearChildren() { Children.Clear(); }
        public override void Abort()
        {
            base.Abort();
            foreach (var child in Children)
            {
                child.Abort();
            }
        }
        public override void Step(IBlackBoard bb, float dt) { status = NodeStatus.SUSPENDED; }
        public override void Enter(IScheduler scheduler)
        {
            CurrentIndex = 0;
            if (Children.Count == 0) Children.Add(new FakeSuccessAction());
            Clear();
            var child = Children[CurrentIndex];
            scheduler.PostSchedule(child);
            child.Enter(scheduler);
        }
    }
}
