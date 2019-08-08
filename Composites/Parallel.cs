using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterDriver
{
    public class Parallel : Composite
    {
        public enum Policy
        {
            Ignore,
            One,
            All
        }
        protected readonly Policy successPolicy, failurePolicy;
        public HashSet<ISchedulable> succeeded = new HashSet<ISchedulable>();
        public HashSet<ISchedulable> failed = new HashSet<ISchedulable>();
        protected IBlackBoard blackBoard;
        public Parallel(IBlackBoard bb, Policy sucPolicy = Policy.Ignore, Policy failPolicy = Policy.One)
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
                    OnComplete(this);
                }
                else if (successPolicy == Policy.All)
                {
                    failed.Remove(sender);
                    succeeded.Add(sender);
                    if (succeeded.Count == Children.Count)
                    {
                        Status = NodeStatus.SUCCESS;
                        AbortChildren();
                        OnComplete(this);
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
                    OnComplete(this);
                }
                else if (failurePolicy == Policy.All)
                {
                    succeeded.Remove(sender);
                    failed.Add(sender);
                    if (failed.Count == Children.Count)
                    {
                        Status = NodeStatus.FAILURE;
                        AbortChildren();
                        OnComplete(this);
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
