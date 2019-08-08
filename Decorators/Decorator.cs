namespace BetterDriver
{
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
}
