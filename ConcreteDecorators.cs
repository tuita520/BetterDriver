using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public class InfiniteDecorator : Decorator
    {
        public InfiniteDecorator(IScheduler s) : base(s) { }
        public override void OnChildCompleted(ISchedulable sender)
        {
            Clear();
            Child.Enter();
        }
    }
    public class RepeatDecorator : Decorator
    {
        protected readonly int times;
        protected int counter;
        public RepeatDecorator(IScheduler s, int t) : base(s) => times = t;
        public override void Enter()
        {
            counter = 0;
            base.Enter();
        }
        public override void OnChildCompleted(ISchedulable sender)
        {
            Status = sender.Status;
            if (Status == NodeStatus.SUCCESS)
            {
                if (++counter < times)
                {
                    Child.Enter();
                }
                else scheduler.OnChildComplete(this);
            }
            else scheduler.OnChildComplete(this);
        }
    }
    public class TimedDecorator : Decorator
    {
        protected readonly float duration;
        protected float counter;
        public TimedDecorator(IScheduler s,  float t) : base(s) => duration = t;
        public override void Step(float dt)
        {
            counter += dt;
            if (counter >= duration)
            {
                Status = NodeStatus.SUCCESS;
                Child.Abort();
            }
            else Status = NodeStatus.RUNNING;
        }
        public override void Enter()
        {
            counter = 0f;
            base.Enter();
        }
        public override void OnChildCompleted(ISchedulable sender)
        {
            Status = sender.Status;
            scheduler.OnChildComplete(this);
        }
    }
}
