using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public static class Extensions
    {
        public static void TryInvoke(this Dictionary<Guid, Action<IScheduler, BehaviorStatus>> d, ISchedulable s, IScheduler c, BehaviorStatus b) { d.TryGetValue(s.ID, out var ret); ret?.Invoke(c, b); }
    }

    public class BehaviorTree : IScheduler
    {
        protected List<Behavior> behaviors = new List<Behavior>();
        protected Queue<ISchedulable> firstQueue = new Queue<ISchedulable>();
        protected Queue<ISchedulable> secondQueue = new Queue<ISchedulable>();
        protected bool CurrentIsFirst = true;
        protected Dictionary<Guid, Action<IScheduler, BehaviorStatus>> callBacks = new Dictionary<Guid, Action<IScheduler, BehaviorStatus>>();

        public Behavior root;
        public void AddBehavior(Behavior b) { behaviors.Add(b); }
        public void PostSchedule(ISchedulable s) { if (CurrentIsFirst) secondQueue.Enqueue(s); else firstQueue.Enqueue(s); }
        public void PostCallBack(ISchedulable schedule, Action<IScheduler, BehaviorStatus> cb) { callBacks[schedule.ID] = cb; }
        public void Terminate(ISchedulable schedule, BehaviorStatus status) { callBacks.TryInvoke(schedule, this, status); }
        public void Enter () { root.Setup(this); }
        public void Leave (BehaviorStatus status) {  }
        public bool Step(float dt)
        {
            if (CurrentIsFirst)
            {
                while (firstQueue.Count > 0)
                {
                    var currentBehavior = firstQueue.Dequeue() as Behavior;
                    var s = currentBehavior.Step(dt);
                    if (s == BehaviorStatus.RUNNING) PostSchedule(currentBehavior);
                    else callBacks.TryInvoke(currentBehavior, this, s);
                }
                
            }
            else
            {
                while (secondQueue.Count > 0)
                {
                    var currentBehavior = secondQueue.Dequeue() as Behavior;
                    var s = currentBehavior.Step(dt);
                    if (s == BehaviorStatus.RUNNING) PostSchedule(currentBehavior);
                    else callBacks.TryInvoke(currentBehavior, this, s);
                }
            }
            CurrentIsFirst = !CurrentIsFirst;
            return true;
        }
    }
}
