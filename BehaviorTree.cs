using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public static class Extensions
    {
        public static T TryGetOrDefault<T>(this Dictionary<string, object> d, string key) => d.TryGetValue(key, out var ret) ? (T)ret : default;
    }

    public class BehaviorTree : IScheduler, IBlackBoard
    {
        protected List<Behavior> behaviors = new List<Behavior>();
        protected Queue<ISchedulable> firstQueue = new Queue<ISchedulable>();
        protected Queue<ISchedulable> secondQueue = new Queue<ISchedulable>();
        protected bool CurrentIsFirst = true;
        protected Dictionary<Guid, SchedulableHandler> onChildCompleted = new Dictionary<Guid, SchedulableHandler>();
        protected Dictionary<string, object> blackBoard = new Dictionary<string, object>();

        public Behavior root;

        private ref Queue<ISchedulable> getCurrentQueue() { if (CurrentIsFirst) return ref firstQueue; else return ref secondQueue; }

        public void AddBehavior(Behavior b) { behaviors.Add(b); }
        public void PostSchedule(ISchedulable s) { if (CurrentIsFirst) secondQueue.Enqueue(s); else firstQueue.Enqueue(s); }
        public void SubscribeChildComplete(ISchedulable child, SchedulableHandler cb) { onChildCompleted[child.ID] = cb; }
        public void UnsubscribeChildComplete(ISchedulable child) { onChildCompleted.Remove(child.ID); }
        public void OnChildComplete(ISchedulable sender) { onChildCompleted.TryGetValue(sender.ID, out var ret); ret?.Invoke(sender); }
        public void Enter() { root.Enter(); Step(0f); }
        public void Leave(NodeStatus status) { }
        public void Step(float dt)
        {
            ref var currentQueue = ref getCurrentQueue();
            while (currentQueue.Count > 0)
            {
                var currentNode = currentQueue.Dequeue();
                var s = currentNode.Status;
                if (s != NodeStatus.ABORTED)
                {
                    currentNode.Step(dt);
                    s = currentNode.Status;
                    if (s == NodeStatus.RUNNING)
                    {
                        PostSchedule(currentNode);
                    }
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
