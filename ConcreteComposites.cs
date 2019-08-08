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
        public HashSet<Guid> succeeded = new HashSet<Guid>();
        public HashSet<Guid> failed = new HashSet<Guid>();
        protected IBlackBoard blackBoard;
        public Parallel(IScheduler s, IBlackBoard bb, Policy sucPolicy = Policy.All, Policy failPolicy = Policy.One) : base(s)
        {
            successPolicy = sucPolicy;
            failurePolicy = failPolicy;
            blackBoard = bb;
        }

        public override void Clear()
        {
            succeeded.Clear();
            failed.Clear();
            base.Clear();
        }
        public override void Enter()
        {
            if (Children.Count == 0) Children.Add(new FakeSuccessAction(scheduler));
            Clear();
            foreach (var child in Children)
            {
                child.Enter();
            }
        }
        public override void OnChildCompleted(ISchedulable sender)
        {
            var s = sender.Status;
            if (s == NodeStatus.SUCCESS)
            {
                if (successPolicy == Policy.One)
                {
                    Status = NodeStatus.SUCCESS;
                    AbortChildren();
                    scheduler.OnChildComplete(this);
                }
                else
                {
                    succeeded.Add(sender.ID);
                    if (succeeded.Count == Children.Count)
                    {
                        Status = NodeStatus.SUCCESS;
                        AbortChildren();
                        scheduler.OnChildComplete(this);
                    }
                    else sender.Enter();
                }
                
            }
            else if (s == NodeStatus.FAILURE)
            {
                if (failurePolicy == Policy.One)
                {
                    Status = NodeStatus.FAILURE;
                    AbortChildren();
                    scheduler.OnChildComplete(this);
                }
                else
                {
                    failed.Add(sender.ID);
                    if (failed.Count == Children.Count)
                    {
                        Status = NodeStatus.FAILURE;
                        AbortChildren();
                        scheduler.OnChildComplete(this);
                    }
                    else sender.Enter();
                }
            }
        }

        protected void AbortChildren()
        {
            foreach (var child in Children)
            {
                if (child.Status == NodeStatus.RUNNING || child.Status == NodeStatus.SUSPENDED) child.Abort();
            }
        }
    }
}
