using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public enum BehaviorStatus
    {
        SUSPENDED,
        SUCCESS,
        FAILURE,
        RUNNING,
        ABORTED
    }
    //base class.
    public abstract class Behavior : IUpdatable, ISchedulable
    {
        private Guid _guid = Guid.NewGuid();
        private BehaviorStatus _status = BehaviorStatus.SUSPENDED;
        public Guid ID
        {
            get { return _guid; }
        }
        public BehaviorStatus Status
        {
            get { return _status; }
            protected set { if (_status != BehaviorStatus.ABORTED) _status = value; }
        }
        public virtual void Abort() { _status = BehaviorStatus.ABORTED; }
        public void Clear() { _status = BehaviorStatus.SUSPENDED; }
        public abstract void Step(IBlackBoard bb, float dt);
        public abstract void Setup(IScheduler scheduler);
        public abstract void OnCompleted(IScheduler scheduler, BehaviorStatus status);
    }

    // leaf nodes.
    public abstract class Action : Behavior
    {
        public override void Setup(IScheduler scheduler) { Clear(); }
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status) { }
    }
    public abstract class Condition : Behavior
    {
        public override void Setup(IScheduler scheduler) { Clear(); }
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status) { }

    }

    //branch nodes.
    public abstract class Decorator : Behavior
    {
        protected Behavior Child;
        public void SetChild(Behavior child) { Child = child; }
        public override void Abort() { base.Abort(); Child.Abort(); }
        public override void Step(IBlackBoard bb, float dt) { Status = BehaviorStatus.SUSPENDED; }
        public override void Setup(IScheduler scheduler)
        {
            if (Child == null) SetChild(new FakeSuccessAction());
            Clear();
            scheduler.PostSchedule(Child);
            scheduler.PostCallBack(Child, OnCompleted);
            Child.Setup(scheduler);
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
        public override void Step(IBlackBoard bb, float dt) { Status = BehaviorStatus.SUSPENDED; }
        public override void Setup(IScheduler scheduler)
        {
            CurrentIndex = 0;
            if (Children.Count == 0) Children.Add(new FakeSuccessAction());
            Clear();
            var child = Children[CurrentIndex];
            scheduler.PostSchedule(child);
            scheduler.PostCallBack(child, OnCompleted);
            child.Setup(scheduler);
        }
    }
}
