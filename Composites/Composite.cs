using System.Collections.Generic;

namespace BetterDriver
{
    public abstract class Composite : Behavior
    {
        protected int CurrentIndex;
        protected List<Behavior> Children = new List<Behavior>();

        public void AddChild(Behavior child) { Children.Add(child);}
        public void RemoveChild(Behavior child) { Children.Remove(child); child.Completed -= OnChildCompleted; }
        public void ClearChildren()
        {
            foreach (var child in Children)
            {
                child.Completed -= OnChildCompleted;
            }
            Children.Clear();
        }
        public override void Init()
        {
            foreach (var child in Children)
            {
                child.Completed += OnChildCompleted;
            }
        }
        public override void Abort()
        {
            base.Abort();
            foreach (var child in Children)
            {
                child.Abort();
            }
        }
        public override void Clear()
        {
            CurrentIndex = 0;
            base.Clear();
        }
        public override void Step(float dt) { Status = NodeStatus.SUSPENDED; }
        public override void Enter()
        {
            Clear();
            var child = Children[CurrentIndex];
            child.Enter();
        }
        public abstract void OnChildCompleted(ISchedulable sender);
    }
}
