using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
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
                    child.Init(scheduler);
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
        public void AddCondition(Behavior condition) { Children.Insert(0, condition); }
        public void AddAction(Behavior action) { Children.Add(action); }
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
                    child.Init(scheduler);
                }
            }
        }
    }
}
