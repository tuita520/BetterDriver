using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterDriver
{
    public class Sequence : Composite
    {
        public Sequence(IScheduler s) : base(s) { }
        public override void OnChildCompleted(ISchedulable sender)
        {
            var s = sender.Status;
            if (s == NodeStatus.SUCCESS)
            {
                if (++CurrentIndex >= Children.Count)
                {
                    Status = NodeStatus.SUCCESS;
                    scheduler.OnChildComplete(this);
                }
                else
                {
                    var child = Children[CurrentIndex];
                    child.Enter();
                }
            }
            else
            {
                Status = s;
                scheduler.OnChildComplete(this);
            }
        }
    }
    public class Filter : Sequence
    {
        public Filter(IScheduler s) : base(s) { }
        public void AddCondition(Behavior condition) { Children.Insert(0, condition); scheduler.SubscribeChildComplete(condition, OnChildCompleted); }
        public void AddAction(Behavior action) { Children.Add(action); scheduler.SubscribeChildComplete(action, OnChildCompleted); }
    }
    public class Selector : Composite
    {
        public Selector(IScheduler s) : base(s) { }
        public override void OnChildCompleted(ISchedulable sender)
        {
            var s = sender.Status;
            if (s == NodeStatus.SUCCESS)
            {
                Status = s;
                scheduler.OnChildComplete(this);
            }
            else
            {
                if (++CurrentIndex >= Children.Count)
                {
                    Status = NodeStatus.FAILURE;
                    scheduler.OnChildComplete(this);
                }
                else
                {
                    var child = Children[CurrentIndex];
                    child.Enter();
                }
            }
        }
    }
    public class Parallel : Composite
    {
        public enum Policy
        {
            One,
            All
        }
        protected readonly Policy successPolicy, failurePolicy;
        protected int successCount, failureCount = 0;
        protected IBlackBoard blackBoard;
        public Parallel(IScheduler s, IBlackBoard bb, Policy sucPolicy = Policy.All, Policy failPolicy = Policy.One) : base(s)
        {
            successPolicy = sucPolicy;
            failurePolicy = failPolicy;
            blackBoard = bb;
        }

        public override void Clear()
        {
            successCount = 0;
            failureCount = 0;
            if (successPolicy == Policy.All && failurePolicy == Policy.All) Status = NodeStatus.RUNNING;
            else Status = NodeStatus.SUSPENDED;
        }
        public override void Enter()
        {
            if (Children.Count == 0) Children.Add(new FakeSuccessAction(scheduler));
            Clear();
            if (successPolicy == Policy.All && failurePolicy == Policy.All) scheduler.PostSchedule(this);
            foreach (var child in Children)
            {
                child.Enter();
            }
        }
        public override void Step(float dt)
        {
            Clear();
        }
        public override void OnChildCompleted(ISchedulable sender)
        {
            var s = sender.Status;
            if (s == NodeStatus.SUCCESS && successPolicy == Policy.One)
            {
                Status = NodeStatus.SUCCESS;
                scheduler.OnChildComplete(this);
                AbortChildren();
            }
            else if (s == NodeStatus.FAILURE && failurePolicy == Policy.One)
            {
                Status = NodeStatus.FAILURE;
                scheduler.OnChildComplete(this);
                AbortChildren();
            }
            if (failurePolicy == Policy.All && s == NodeStatus.FAILURE)
            {
                ++failureCount;
                if (failureCount == Children.Count)
                {
                    Status = NodeStatus.FAILURE;
                    scheduler.OnChildComplete(this);
                    AbortChildren();
                }
            }
            else if (successPolicy == Policy.All && s == NodeStatus.SUCCESS)
            {
                ++successCount;
                if (successCount == Children.Count)
                {
                    Status = NodeStatus.SUCCESS;
                    scheduler.OnChildComplete(this);
                    AbortChildren();
                }
            }
        }

        protected void AbortChildren()
        {
            foreach (var child in Children)
            {
                if (child.Status == NodeStatus.RUNNING) child.Abort();
            }
        }
    }
}
