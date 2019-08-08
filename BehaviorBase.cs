using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BetterDriver
{
    //base class.
    public abstract class Behavior : ISchedulable, IObserver
    {
        public event SchedulableHandler Completed;
        public NodeStatus Status { get; protected set; }
        [JsonIgnore]
        public Guid ID { get; }

        public Behavior() => ID = Guid.NewGuid();
        public void OnComplete(ISchedulable sender) { Completed?.Invoke(sender); }
        public virtual void Clear() { Status = NodeStatus.SUSPENDED; }
        public virtual void Abort() { Status = NodeStatus.ABORTED; }

        public abstract void Step(float dt);
        public abstract void Enter();
        public abstract void Init();
    }

    // leaf nodes.
    public abstract class Action : Behavior
    {
        protected IScheduler scheduler;
        public Action(IScheduler s) => scheduler = s;
        public override void Enter() { Clear(); scheduler.PostSchedule(this); }

        public override void Init() { }
    }
    public abstract class Condition : Behavior
    {
        protected IScheduler scheduler;
        public Condition(IScheduler s) => scheduler = s;
        public override void Enter() { Clear(); scheduler.PostSchedule(this); }
        public override void Init() { }
    }

    //branch nodes.
    public abstract class Decorator : Behavior
    {
        protected Behavior Child;

        public void SetChild(Behavior child) { Child = child; }
        public override void Init() { Child.Completed += OnChildCompleted; }
        public override void Abort() { base.Abort(); Child.Abort(); }
        public override void Step(float dt) { Status = NodeStatus.SUSPENDED; }
        public override void Enter()
        {
            Clear();
            Child.Enter();
        }
        public abstract void OnChildCompleted(ISchedulable sender);
    }

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
