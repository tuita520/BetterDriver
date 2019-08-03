using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public class BehaviorTree<T>
    {
        public T context;
        protected List<Behavior> behaviors = new List<Behavior>();
        public Behavior root;
        public void AddBehavior(Behavior b) { behaviors.Add(b); }
        public void Enter () { root.Enter(); }
        public void Leave (BehaviorStatus status) { root.Leave(status); }
        public BehaviorStatus Step(float dt)
        {
            return root.Step(dt);
        }
    }
}
