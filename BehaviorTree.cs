using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public static class Extensions
    {
        public static void TryInvoke(this Dictionary<Guid, Action<IScheduler, BehaviorStatus>> d, ISchedulable s, IScheduler c, BehaviorStatus b) { d.TryGetValue(s.ID, out var ret); ret?.Invoke(c, b); }
        public static T TryGetOrDefault<T>(this Dictionary<string, object> d, string key) => d.TryGetValue(key, out var ret) ? (T)ret : default(T);
    }

    public class BehaviorTree : IScheduler, IBlackBoard
    {
        protected List<Behavior> behaviors = new List<Behavior>();
        protected Queue<ISchedulable> firstQueue = new Queue<ISchedulable>();
        protected Queue<ISchedulable> secondQueue = new Queue<ISchedulable>();
        protected bool CurrentIsFirst = true;
        protected Dictionary<Guid, Action<IScheduler, BehaviorStatus>> onCompleted = new Dictionary<Guid, Action<IScheduler, BehaviorStatus>>();
        protected Dictionary<string, object> blackBoard = new Dictionary<string, object>();

        public Behavior root;

        private ref Queue<ISchedulable> getCurrentQueue() { if (CurrentIsFirst) return ref firstQueue; else return ref secondQueue; }

        public void AddBehavior(Behavior b) { behaviors.Add(b); }
        public void PostSchedule(ISchedulable s) { if (CurrentIsFirst) secondQueue.Enqueue(s); else firstQueue.Enqueue(s); }
        public void PostCallBack(ISchedulable schedule, Action<IScheduler, BehaviorStatus> cb) { onCompleted[schedule.ID] = cb; }
        public void Terminate(ISchedulable schedule, BehaviorStatus status) { onCompleted.TryInvoke(schedule, this, status); }
        public void Enter() { root.Setup(this); Step(0f); }
        public void Leave(BehaviorStatus status) { }
        public void Step(float dt)
        {
            ref var currentQueue = ref getCurrentQueue();
            while (currentQueue.Count > 0)
            {
                var currentBehavior = currentQueue.Dequeue() as Behavior;
                currentBehavior.Step(this, dt);
                var s = currentBehavior.Status;
                if (s == BehaviorStatus.RUNNING) PostSchedule(currentBehavior);
                else if (s != BehaviorStatus.SUSPENDED) onCompleted.TryInvoke(currentBehavior, this, s);
            }
            CurrentIsFirst = !CurrentIsFirst;
        }

        public T Get<T>(string key)
        {
            return blackBoard.TryGetOrDefault<T>(key);
        }
        public void Post<T>(string key, T value)
        {
            blackBoard[key] = value;
        }
    }
}
