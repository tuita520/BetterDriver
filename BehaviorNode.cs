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
    //base class.
    public abstract class Behavior
    {
        public abstract void Enter();
        public abstract BehaviorStatus Step(float dt);
        public abstract void Leave(BehaviorStatus status);
    }
    //interfaces.
    public interface IComposable
    {
        void AddChild(Behavior c);
        void RemoveChild(Behavior c);
        void ClearChildren();
    }
    // fake objects.
    public class FakeSuccessAction : Action
    {
        public static readonly FakeSuccessAction Instance = new FakeSuccessAction();
        public override void Enter() { }
        public override void Leave(BehaviorStatus status) { }
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.SUCCESS; }
    }
    public class FakeFailureAction : Action
    {
        public static readonly FakeFailureAction Instance = new FakeFailureAction();
        public override void Enter() { }
        public override void Leave(BehaviorStatus status) { }
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.FAILURE; }
    }
    public class AlwaysTrueCondition : Condition
    {
        public static readonly AlwaysTrueCondition Instance = new AlwaysTrueCondition();
        public override void Enter() { }
        public override void Leave(BehaviorStatus status) { }
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.SUCCESS; }
    }
    public class AlwaysFalseCondition : Condition
    {
        public static readonly AlwaysFalseCondition Instance = new AlwaysFalseCondition();
        public override void Enter() { }
        public override void Leave(BehaviorStatus status) { }
        public override BehaviorStatus Step(float dt) { return BehaviorStatus.FAILURE; }
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
        public void SetChild (Behavior child) { Child = child; }
        public override void Enter() { if (Child == null) SetChild(FakeSuccessAction.Instance); Child.Enter(); }
        public override void Leave(BehaviorStatus status) { Child.Leave(status); }
    }

    public abstract class Composite : Behavior, IComposable
    {
        protected int CurrentIndex;
        protected List<Behavior> Children = new List<Behavior>();
        public override void Enter() { CurrentIndex = 0; if (Children.Count == 0) Children.Add(FakeSuccessAction.Instance); Children[CurrentIndex].Enter(); }
        public void AddChild(Behavior child) { Children.Add(child); }
        public void RemoveChild(Behavior child) { Children.Remove(child); }
        public void ClearChildren() { Children.Clear(); }
    }

    public class Sequence : Composite
    {
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
        public void AddCondition (Behavior condition) { Children.Insert(0, condition); }
        public void AddAction (Behavior action) { Children.Add(action); }
    }

    public class Selector : Composite
    {
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
    
}
