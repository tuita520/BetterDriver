using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public class BehaviorTree<T>
    {
        public T context;
        public List<Behavior> nodes = new List<Behavior>();
        public Behavior root;
        public void Enter () { }
        public void Leave (BehaviorStatus status) { }
        public BehaviorStatus Step(float dt)
        {
            return root.Step(dt);
        }
    }
}
