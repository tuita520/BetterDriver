using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    // fake objects.
    public class FakeSuccessAction : Action
    {
        public override void Step(IBlackBoard bb, float dt) { Status = BehaviorStatus.SUCCESS; }
    }
    public class FakeFailureAction : Action
    {
        public override void Step(IBlackBoard bb, float dt) { Status = BehaviorStatus.FAILURE; }
    }
    public class AlwaysTrueCondition : Condition
    {
        public override void Step(IBlackBoard bb, float dt) { Status = BehaviorStatus.SUCCESS; }
    }
    public class AlwaysFalseCondition : Condition
    {
        public override void Step(IBlackBoard bb, float dt) { Status = BehaviorStatus.FAILURE; }
    }
}
