using System.Collections.Generic;

namespace BetterDriver
{
    public static class Extensions
    {
        public static T TryGetOrDefault<T>(this Dictionary<string, object> d, string key) => d.TryGetValue(key, out var ret) ? (T)ret : default;
    }

    public class BehaviorTree : IScheduler, IBlackBoard
    {
        protected Queue<ISchedulable> firstQueue = new Queue<ISchedulable>();
        protected Queue<ISchedulable> secondQueue = new Queue<ISchedulable>();
        protected bool CurrentIsFirst = true;
        protected Dictionary<string, object> blackBoard = new Dictionary<string, object>();

        public List<Behavior> behaviors = new List<Behavior>();
        public Behavior root;

        private ref Queue<ISchedulable> getCurrentQueue() { if (CurrentIsFirst) return ref firstQueue; else return ref secondQueue; }

        public void AddBehavior(Behavior b) { behaviors.Add(b); }
        public void PostSchedule(ISchedulable s) { if (CurrentIsFirst) secondQueue.Enqueue(s); else firstQueue.Enqueue(s); }
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
