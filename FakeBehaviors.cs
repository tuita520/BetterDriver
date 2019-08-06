using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    // fake objects.
    public class FakeSuccessAction : Action
    {
        public override void Step(IBlackBoard bb, float dt) { status = NodeStatus.SUCCESS; }
    }
    public class FakeFailureAction : Action
    {
        public override void Step(IBlackBoard bb, float dt) { status = NodeStatus.FAILURE; }
    }
    public class AlwaysTrueCondition : Condition
    {
        public override void Step(IBlackBoard bb, float dt) { status = NodeStatus.SUCCESS; }
    }
    public class AlwaysFalseCondition : Condition
    {
        public override void Step(IBlackBoard bb, float dt) { status = NodeStatus.FAILURE; }
    }
}
