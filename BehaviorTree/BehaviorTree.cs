using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public static class Extensions
    {
        public static void TryInvoke(this Dictionary<Guid, Action<IScheduler, NodeStatus>> d, ISchedulable s, IScheduler c, NodeStatus b) { d.TryGetValue(s.ID, out var ret); ret?.Invoke(c, b); }
        public static T TryGetOrDefault<T>(this Dictionary<string, object> d, string key) => d.TryGetValue(key, out var ret) ? (T)ret : default;
    }

    public class BehaviorTree : IScheduler, IBlackBoard
    {
        protected List<Behavior> behaviors = new List<Behavior>();
        protected Queue<ISchedulable> firstQueue = new Queue<ISchedulable>();
        protected Queue<ISchedulable> secondQueue = new Queue<ISchedulable>();
        protected bool CurrentIsFirst = true;
        protected Dictionary<Guid, Action<IScheduler, NodeStatus>> onCompleted = new Dictionary<Guid, Action<IScheduler, NodeStatus>>();
        protected Dictionary<string, object> blackBoard = new Dictionary<string, object>();

        public Behavior root;

        private ref Queue<ISchedulable> getCurrentQueue() { if (CurrentIsFirst) return ref firstQueue; else return ref secondQueue; }

        public void AddBehavior(Behavior b) { behaviors.Add(b); }
        public void PostSchedule(ISchedulable s) { if (CurrentIsFirst) secondQueue.Enqueue(s); else firstQueue.Enqueue(s); }
        public void PostCallBack(ISchedulable schedule, Action<IScheduler, NodeStatus> cb) { onCompleted[schedule.ID] = cb; }
        public void Terminate(ISchedulable schedule, NodeStatus status) { onCompleted.TryInvoke(schedule, this, status); }
        public void Enter() { root.Init(this); Step(0f); }
        public void Leave(NodeStatus status) { }
        public void Step(float dt)
        {
            ref var currentQueue = ref getCurrentQueue();
            while (currentQueue.Count > 0)
            {
                var currentNode = currentQueue.Dequeue();
                ref readonly var s = ref currentNode.Status;
                if (s != NodeStatus.ABORTED)
                {
                    currentNode.Step(this, dt);
                    if (s == NodeStatus.RUNNING) PostSchedule(currentNode);
                    else if (s == NodeStatus.SUCCESS || s == NodeStatus.FAILURE) onCompleted.TryInvoke(currentNode, this, s);
                }
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
