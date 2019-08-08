
namespace BetterDriver
{
    // fake objects.
    public class FakeSuccessAction : Action
    {
        public FakeSuccessAction(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.SUCCESS; OnComplete(this); }
    }
    public class FakeFailureAction : Action
    {
        public FakeFailureAction(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.FAILURE; OnComplete(this); }
    }
    public class AlwaysTrueCondition : Condition
    {
        public AlwaysTrueCondition(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.SUCCESS; OnComplete(this); }
    }
    public class AlwaysFalseCondition : Condition
    {
        public AlwaysFalseCondition(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.FAILURE; OnComplete(this); }
    }
}
