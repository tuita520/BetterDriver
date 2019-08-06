using System.Collections.Generic;
using XiaWorld;

namespace BetterDriver
{
    public class NpcBehaviorBuilder : BehaviorTreeBuilder<NpcBehaviorBuilder>
    {
        protected override NpcBehaviorBuilder BuilderInstance => this;
        public NpcBehaviorBuilder(Npc t) { result.Post("Caster", t); }
        public NpcBehaviorBuilder IsInFightCondition()
        {
            var cond = new NpcIsInFightCondition();
            AddLeaf(cond);
            return this;
        }
        public NpcBehaviorBuilder CastSkillAction(string skill)
        {
            var act = new NpcCastSkillAction(skill);
            AddLeaf(act);
            return this;
        }
        public NpcBehaviorBuilder CanCastSkillCondition(string skill)
        {
            var cond = new NpcCanCastSkillCondition(skill);
            AddLeaf(cond);
            return this;
        }
        public NpcBehaviorBuilder CastFilters(IEnumerable<string> skills)
        {
            foreach (string skill in skills)
            {
                Filter();
                CanCastSkillCondition(skill);
                CastSkillAction(skill);
                End();
            }
            return this;
        }
    }
}
