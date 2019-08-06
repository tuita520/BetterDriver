using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public class InfiniteDecorator : Decorator
    {
        public override void OnCompleted(IScheduler scheduler, NodeStatus status)
        {
            Clear();
            scheduler.PostSchedule(Child);
            Child.Init(scheduler);
        }
    }
    public class RepeatDecorator : Decorator
    {
        protected readonly int times;
        protected int counter;
        public RepeatDecorator(int t) => times = t;
        public override void Init(IScheduler scheduler)
        {
            counter = 0;
            base.Init(scheduler);
        }
        public override void OnCompleted(IScheduler scheduler, NodeStatus status)
        {
            if (status == NodeStatus.SUCCESS)
            {
                if (++counter < times)
                {
                    scheduler.PostSchedule(Child);
                    Child.Init(scheduler);
                }
                else scheduler.Terminate(this, NodeStatus.SUCCESS);
            }
            else scheduler.Terminate(this, status);
        }
    }
    public class TimedDecorator : Decorator
    {
        protected readonly float duration;
        protected float counter;
        public TimedDecorator(float t) => duration = t;
        public override void Step(IBlackBoard bb, float dt)
        {
            counter += dt;
            if (counter >= duration)
            {
                status = NodeStatus.SUCCESS;
                Child.Abort();
            }
            else status = NodeStatus.RUNNING;
        }
        public override void Init(IScheduler scheduler)
        {
            counter = 0f;
            base.Init(scheduler);
        }
        public override void OnCompleted(IScheduler scheduler, NodeStatus status)
        {
            scheduler.Terminate(this, status);
        }
    }
}
