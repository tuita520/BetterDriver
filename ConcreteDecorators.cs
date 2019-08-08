using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public class InfiniteDecorator : Decorator
    {
        public override void OnChildCompleted(ISchedulable sender)
        {
            Clear();
            Child.Enter();
        }
    }
    public class IgnoreFailureDecorator : Decorator
    {
        public override void OnChildCompleted(ISchedulable sender)
        {
            Status = NodeStatus.SUCCESS;
            OnComplete(this);
        }
    }
    public class InvertDecorator : Decorator
    {

        public override void OnChildCompleted(ISchedulable sender)
        {
            var s = sender.Status;
            if (s == NodeStatus.SUCCESS) Status = NodeStatus.FAILURE;
            else Status = NodeStatus.SUCCESS;
            OnComplete(this);
        }
    }
    public class RepeatDecorator : Decorator
    {
        protected readonly int times;
        protected int counter;
        public RepeatDecorator(int t) => times = t;
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
                else OnComplete(this);
            }
            else OnComplete(this);
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
            OnComplete(this);
        }
    }
}
