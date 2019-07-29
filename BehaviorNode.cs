using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public enum BehaviorStatus
    {
        SUCCESS,
        FAILURE,
        RUNNING,
        SUSPENDED,
        ABORTED
    }
    public abstract class Behavior
    {
        public Behavior Parent;
        public abstract void Enter();
        public abstract BehaviorStatus Step(float dt);
        public abstract void Leave(BehaviorStatus status);
    }

    // leaf nodes.
    public abstract class Action : Behavior
    {
        
    }
    public abstract class Condition : Behavior
    {
        
    }

    //branch nodes.
    public abstract class Decorator : Behavior
    {
        protected Behavior Child;
        public void SetChild (Behavior child) { Child = child; child.Parent = this; }
        public override void Enter() { Child.Enter(); }
        public override void Leave(BehaviorStatus status) { Child.Leave(status); }
    }

    public abstract class Composite : Behavior
    {
        protected List<Behavior> Children = new List<Behavior>();
        public void AddChild(Behavior child) { Children.Add(child); child.Parent = this; }
        public void RemoveChild(Behavior child) { Children.Remove(child); }
        public void ClearChildren() { Children.Clear(); }
    }

    public class Sequence : Composite
    {
        protected int CurrentIndex;
        public override void Enter() { CurrentIndex = 0; Children?[CurrentIndex].Enter(); }
        public override BehaviorStatus Step(float dt)
        {
            while (true)
            {
                var cur_child = Children[CurrentIndex];
                var s = cur_child.Step(dt);
                if (s == BehaviorStatus.FAILURE) cur_child.Leave(BehaviorStatus.FAILURE);
                if (s != BehaviorStatus.SUCCESS) return s;
                cur_child.Leave(BehaviorStatus.SUCCESS);
                if (++CurrentIndex >= Children.Count) return BehaviorStatus.SUCCESS;
                Children[CurrentIndex].Enter();
            }
        }
        public override void Leave(BehaviorStatus status) { }
    }
    public class Filter : Sequence
    {
        public void AddCondition (Behavior condition) { Children.Insert(0, condition); condition.Parent = this; }
        public void AddAction (Behavior action) { Children.Add(action); action.Parent = this; }
    }

    public class Selector : Composite
    {
        protected int CurrentIndex;
        public override void Enter() { CurrentIndex = 0; Children?[CurrentIndex].Enter(); }
        public override BehaviorStatus Step(float dt)
        {
            while (true)
            {
                var cur_child = Children[CurrentIndex];
                var s = cur_child.Step(dt);
                if (s == BehaviorStatus.SUCCESS) cur_child.Leave(BehaviorStatus.SUCCESS);
                if (s != BehaviorStatus.FAILURE) return s;
                cur_child.Leave(BehaviorStatus.FAILURE);
                if (++CurrentIndex >= Children.Count) return BehaviorStatus.FAILURE;
                Children[CurrentIndex].Enter();
            }
        }
        public override void Leave(BehaviorStatus status) { }
    }

    // it will reset its child once it finishes. Always return running status.
    public class RootDecorator : Decorator
    {
        public RootDecorator() { Parent = this; }
        public override BehaviorStatus Step(float dt)
        {
            var s = Child.Step(dt);
            if (s != BehaviorStatus.RUNNING) Child.Enter();
            return BehaviorStatus.RUNNING;
        }
    }
    public class RepeatDecorator : Decorator
    {
        protected readonly int times;
        protected int counter;
        public RepeatDecorator(int t) { times = t; }
        public override void Enter()
        {
            counter = 0;
            base.Enter();
        }
        public override BehaviorStatus Step(float dt)
        {
            while (true)
            {
                var s = Child.Step(dt);
                if (s != BehaviorStatus.SUCCESS) return s;
                if (++counter >= times) return BehaviorStatus.SUCCESS;
                Child.Enter();
            }
        }
    }
    public class MockAction : Action
    {
        public override void Enter() { }
        public override void Leave(BehaviorStatus status) { }
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.SUCCESS; }
    }
}
