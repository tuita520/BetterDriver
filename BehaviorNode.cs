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
        private Guid _guid = Guid.NewGuid();
        private BehaviorStatus _status;
        public Guid ID
        {
            get { return _guid; }
        }
        public BehaviorStatus Status
        {
            get { return _status; }
            protected set
            {
                if (_status == BehaviorStatus.ABORTED) return;
                else _status = value;
            }
        }
        public void Abort() { Status = BehaviorStatus.ABORTED; }
        public abstract void Step(float dt);
        public abstract void Setup(IScheduler scheduler);
        public abstract void OnCompleted(IScheduler scheduler, BehaviorStatus status);
    }
    // fake objects.
    public class FakeSuccessAction : Action
    {
        public override void Step(float dt) { Status = BehaviorStatus.SUCCESS; }
    }
    public class FakeFailureAction : Action
    {
        public override void Step(float dt) { Status = BehaviorStatus.FAILURE; }
    }
    public class AlwaysTrueCondition : Condition
    {
        public override void Step(float dt) { Status = BehaviorStatus.SUCCESS; }
    }
    public class AlwaysFalseCondition : Condition
    {
        public override void Step(float dt) { Status = BehaviorStatus.FAILURE; }
    }

    // leaf nodes.
    public abstract class Action : Behavior
    {
        public override void Setup(IScheduler scheduler) { }
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status) { }
    }
    public abstract class Condition : Behavior
    {
        public override void Setup(IScheduler scheduler) { }
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status) { }

    }

    //branch nodes.
    public abstract class Decorator : Behavior
    {
        protected Behavior Child;
        public void SetChild (Behavior child) { Child = child; }
        public override void Step(float dt) { Status = BehaviorStatus.SUSPENDED; }
        public override void Setup(IScheduler scheduler)
        {
            if (Child == null) SetChild(new FakeSuccessAction());
            scheduler.PostSchedule(Child);
            scheduler.PostCallBack(Child, OnCompleted);
            Child.Setup(scheduler);
        }
    }

    public abstract class Composite : Behavior, IComposable
    {
        protected int CurrentIndex;
        protected List<Behavior> Children = new List<Behavior>();
        public void AddChild(Behavior child) { Children.Add(child); }
        public void RemoveChild(Behavior child) { Children.Remove(child); }
        public void ClearChildren() { Children.Clear(); }
        public override void Step(float dt) { Status = BehaviorStatus.SUSPENDED; }
        public override void Setup(IScheduler scheduler)
        {
            CurrentIndex = 0;
            if (Children.Count == 0) Children.Add(new FakeSuccessAction());
            var child = Children[CurrentIndex];
            scheduler.PostSchedule(child);
            scheduler.PostCallBack(child, OnCompleted);
            child.Setup(scheduler);
        }
    }

    // concrete branches.
    public class Sequence : Composite
    {
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status)
        {
            if (status == BehaviorStatus.SUCCESS)
            {
                if (++CurrentIndex >= Children.Count)
                {
                    scheduler.Terminate(this, BehaviorStatus.SUCCESS);
                }
                else
                {
                    var child = Children[CurrentIndex];
                    scheduler.PostSchedule(child);
                    scheduler.PostCallBack(child, OnCompleted);
                    child.Setup(scheduler);
                }
            }
            else
            {
                scheduler.Terminate(this, status);
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
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status)
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
                    scheduler.PostCallBack(child, OnCompleted);
                    child.Setup(scheduler);
                }
            }
        }
    }
    public class RootDecorator : Decorator
    {
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status)
        {
            scheduler.PostSchedule(Child);
            scheduler.PostCallBack(Child, OnCompleted);
            Child.Setup(scheduler);
        }
    }
    public class RepeatDecorator : Decorator
    {
        protected readonly int times;
        protected int counter;
        public RepeatDecorator(int t) => times = t;
        public override void Setup(IScheduler scheduler)
        {
            counter = 0;
            base.Setup(scheduler);
        }
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status)
        {
            if (status == BehaviorStatus.SUCCESS)
            {
                if (++counter < times)
                {
                    scheduler.PostSchedule(Child);
                    scheduler.PostCallBack(Child, OnCompleted);
                    Child.Setup(scheduler);
                }
                else scheduler.Terminate(this, BehaviorStatus.SUCCESS);
            }
            else scheduler.Terminate(this, status);
        }
    }
    public class TimedDecorator : Decorator
    {
        protected readonly float duration;
        protected float counter;
        public TimedDecorator(float t) => duration = t;
        public override void Step(float dt)
        {
            counter += dt;
            if (counter >= duration) Child.Abort();
            else Status = BehaviorStatus.RUNNING;
        }
        public override void Setup(IScheduler scheduler)
        {
            counter = 0f;
            base.Setup(scheduler);
        }
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status)
        {
            Status = status;
        }
    }
    
}
