using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public class RootDecorator : Decorator
    {
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status)
        {
            Clear();
            scheduler.PostSchedule(Child);
            scheduler.PostCallBack(Child, OnCompleted);
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
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status)
        {
            if (status == BehaviorStatus.SUCCESS)
            {
                if (++counter < times)
                {
                    scheduler.PostSchedule(Child);
                    scheduler.PostCallBack(Child, OnCompleted);
                    Child.Init(scheduler);
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
        public override void Step(IBlackBoard bb, float dt)
        {
            counter += dt;
            if (counter >= duration) Abort();
            else Status = BehaviorStatus.RUNNING;
        }
        public override void Init(IScheduler scheduler)
        {
            counter = 0f;
            base.Init(scheduler);
        }
        public override void OnCompleted(IScheduler scheduler, BehaviorStatus status)
        {
            Status = status;
        }
    }
}
