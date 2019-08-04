using System;
using XiaWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    
    public class NpcCastSkillAction : Action
    {
        private Npc caster;
        private string skill;
        public NpcCastSkillAction(Npc t, string n) { caster = t; skill = n;  }
        public override void Step(float dt)
        {
            caster.CastSkill(skill, caster.FightBody?.Target, 0);
            Status = BehaviorStatus.SUCCESS;
        }
    }

    public class NpcCanCastSkillCondition : Condition
    {
        private Npc caster;
        private string skill;
        public NpcCanCastSkillCondition(Npc t, string n) { caster = t; skill = n; }

        public override void Step(float dt)
        {
            if (caster?.FightBody?.Casting?.Skill != skill && caster?.FightBody?.CheckSkillCast(skill) == true)
            {
                Status = BehaviorStatus.SUCCESS;
            }
            else
            {
                Status = BehaviorStatus.FAILURE;
            }
        }
    }

    public class NpcIsInFightCondition : Condition
    {
        private Npc caster;
        public NpcIsInFightCondition(Npc t) { caster = t; }
        public override void Step(float dt)
        {
            if (caster?.FightBody?.IsFighting == true)
            {
                Status = BehaviorStatus.SUCCESS;
            }
            else
            {
                Status = BehaviorStatus.FAILURE;
            }
        }
    }

}
