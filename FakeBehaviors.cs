using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    // fake objects.
    public class FakeSuccessAction : Action
    {
        public FakeSuccessAction(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.SUCCESS; scheduler.OnChildComplete(this); }
    }
    public class FakeFailureAction : Action
    {
        public FakeFailureAction(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.FAILURE; scheduler.OnChildComplete(this); }
    }
    public class AlwaysTrueCondition : Condition
    {
        public AlwaysTrueCondition(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.SUCCESS; scheduler.OnChildComplete(this); }
    }
    public class AlwaysFalseCondition : Condition
    {
        public AlwaysFalseCondition(IScheduler s) : base(s) { }
        public override void Step(float dt) { Status = NodeStatus.FAILURE; scheduler.OnChildComplete(this); }
    }
}
