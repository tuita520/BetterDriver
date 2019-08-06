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
            Child.Enter(scheduler);
        }
    }
    public class RepeatDecorator : Decorator
    {
        protected readonly int times;
        protected int counter;
        public RepeatDecorator(int t) => times = t;
        public override void Enter(IScheduler scheduler)
        {
            counter = 0;
            base.Enter(scheduler);
        }
        public override void OnCompleted(IScheduler scheduler, NodeStatus status)
        {
            if (status == NodeStatus.SUCCESS)
            {
                if (++counter < times)
                {
                    scheduler.PostSchedule(Child);
                    Child.Enter(scheduler);
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
        public override void Step(float dt)
        {
            counter += dt;
            if (counter >= duration)
            {
                status = NodeStatus.SUCCESS;
                Child.Abort();
            }
            else status = NodeStatus.RUNNING;
        }
        public override void Enter(IScheduler scheduler)
        {
            counter = 0f;
            base.Enter(scheduler);
        }
        public override void OnCompleted(IScheduler scheduler, NodeStatus status)
        {
            scheduler.Terminate(this, status);
        }
    }
}
