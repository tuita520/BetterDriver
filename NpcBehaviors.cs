using System;
using XiaWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    
    public class NpcCastSkillAction : Action
    {
        private string skill;
        public NpcCastSkillAction(string s) => skill = s;
        public override void Step(IBlackBoard bb, float dt)
        {
            var caster = bb.Get<Npc>("Caster");
            caster.CastSkill(skill, caster.FightBody?.Target, 0);
            status = NodeStatus.SUCCESS;
        }
    }

    public class NpcCanCastSkillCondition : Condition
    {
        private string skill;
        public NpcCanCastSkillCondition(string s) => skill = s;
        public override void Step(IBlackBoard bb, float dt)
        {
            var caster = bb.Get<Npc>("Caster");
            if (caster?.FightBody?.Casting?.Skill != skill && caster?.FightBody?.CheckSkillCast(skill) == true)
            {
                status = NodeStatus.SUCCESS;
            }
            else
            {
                status = NodeStatus.FAILURE;
            }
        }
    }

    public class NpcIsInFightCondition : Condition
    {
        public override void Step(IBlackBoard bb, float dt)
        {
            var caster = bb.Get<Npc>("Caster");
            if (caster?.FightBody?.IsFighting == true)
            {
                status = NodeStatus.SUCCESS;
            }
            else
            {
                status = NodeStatus.FAILURE;
            }
        }
    }

}
