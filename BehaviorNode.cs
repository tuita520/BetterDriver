using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public enum BehaviorStatus
    {
        SUCCESS,
        FAILURE,
        RUNNING,
        SUSPENDED,
        ABORTED
    }
    //base class.
    public abstract class Behavior : IUpdatable, ISchedulable
    {
        protected Guid _guid = Guid.NewGuid();
        public Guid ID
        {
            get { return _guid; }
        }
        public abstract BehaviorStatus Step(float dt);
        public abstract void Setup(IScheduler scheduler);
    }
    // fake objects.
    public class FakeSuccessAction : Action
    {
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.SUCCESS; }
    }
    public class FakeFailureAction : Action
    {
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.FAILURE; }
    }
    public class AlwaysTrueCondition : Condition
    {
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.SUCCESS; }
    }
    public class AlwaysFalseCondition : Condition
    {
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.FAILURE; }
    }

    // leaf nodes.
    public abstract class Action : Behavior
    {
        public override void Setup(IScheduler scheduler) { }
    }
    public abstract class Condition : Behavior
    {
        public override void Setup(IScheduler scheduler) { }

    }

    //branch nodes.
    public abstract class Decorator : Behavior
    {
        protected Behavior Child;
        public void SetChild (Behavior child) { Child = child; }
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.SUSPENDED; }
        public override void Setup(IScheduler scheduler)
        {
            if (Child == null) SetChild(new FakeSuccessAction());
            scheduler.PostSchedule(Child);
            scheduler.PostCallBack(Child, OnComplete);
            Child.Setup(scheduler);
        }
        protected abstract void OnComplete(IScheduler scheduler, BehaviorStatus status);
    }

    public abstract class Composite : Behavior, IComposable
    {
        protected int CurrentIndex;
        protected List<Behavior> Children = new List<Behavior>();
        public void AddChild(Behavior child) { Children.Add(child); }
        public void RemoveChild(Behavior child) { Children.Remove(child); }
        public void ClearChildren() { Children.Clear(); }
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.SUSPENDED; }
        public override void Setup(IScheduler scheduler)
        {
            CurrentIndex = 0;
            if (Children.Count == 0) Children.Add(new FakeSuccessAction());
            var child = Children[CurrentIndex];
            scheduler.PostSchedule(child);
            scheduler.PostCallBack(child, OnComplete);
            child.Setup(scheduler);
        }
        protected abstract void OnComplete(IScheduler scheduler, BehaviorStatus status);
    }

    // concrete branches.
    public class Sequence : Composite
    {
        protected override void OnComplete(IScheduler scheduler, BehaviorStatus status)
        {
            if (status == BehaviorStatus.FAILURE)
            {
                scheduler.Terminate(this, status);
            }
            else
            {
                if (++CurrentIndex >= Children.Count)
                {
                    scheduler.Terminate(this, BehaviorStatus.SUCCESS);
                }
                else
                {
                    var child = Children[CurrentIndex];
                    scheduler.PostSchedule(child);
                    scheduler.PostCallBack(child, OnComplete);
                    child.Setup(scheduler);
                }
            }
        }
    }
    public class Filter : Sequence
    {
        public void AddCondition (Behavior condition) { Children.Insert(0, condition); }
        public void AddAction (Behavior action) { Children.Add(action); }
    }

    public class Selector : Composite
    {
        protected override void OnComplete(IScheduler scheduler, BehaviorStatus status)
        {
            if (status == BehaviorStatus.SUCCESS)
            {
                scheduler.Terminate(this, status);
            }
            else
            {
                if (++CurrentIndex >= Children.Count)
                {
                    scheduler.Terminate(this, BehaviorStatus.FAILURE);
                }
                else
                {
                    var child = Children[CurrentIndex];
                    scheduler.PostSchedule(child);
                    scheduler.PostCallBack(child, OnComplete);
                    child.Setup(scheduler);
                }
            }
        }
    }

    // it will reset its child once it finishes. Always return running status.
    public class RootDecorator : Decorator
    {
        protected override void OnComplete(IScheduler scheduler, BehaviorStatus status)
        {
            scheduler.PostSchedule(Child);
            scheduler.PostCallBack(Child, OnComplete);
            Child.Setup(scheduler);
        }
    }
    public class RepeatDecorator : Decorator
    {
        protected readonly int times;
        protected int counter;
        public RepeatDecorator(int t) { times = t; }
        public override void Setup(IScheduler scheduler)
        {
            counter = 0;
            base.Setup(scheduler);
        }
        protected override void OnComplete(IScheduler scheduler, BehaviorStatus status)
        {
            if (status == BehaviorStatus.SUCCESS)
            {
                if (++counter < times)
                {
                    scheduler.PostSchedule(Child);
                    scheduler.PostCallBack(Child, OnComplete);
                    Child.Setup(scheduler);
                }
                else scheduler.Terminate(this, BehaviorStatus.SUCCESS);
            }
            else scheduler.Terminate(this, BehaviorStatus.FAILURE);
        }
    }
    
}
