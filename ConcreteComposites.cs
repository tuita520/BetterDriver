using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    scheduler.PostSchedule(child);
                    child.Enter(scheduler);
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
                    scheduler.PostSchedule(child);
                    child.Enter(scheduler);
                }
            }
        }
    }
}
