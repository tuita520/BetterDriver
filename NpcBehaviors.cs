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
        public override void Enter() { }
        public override void Leave(BehaviorStatus status) { }
        public override BehaviorStatus Step(float dt)
        {
            caster.CastSkill(skill, caster.FightBody?.Target, 0);
            return BehaviorStatus.SUCCESS;
        }
    }

    public class NpcCanCastSkillCondition : Condition
    {
        private Npc caster;
        private string skill;
        public NpcCanCastSkillCondition(Npc t, string n) { caster = t; skill = n; }
        public override void Enter() { }

        public override void Leave(BehaviorStatus status) { }

        public override BehaviorStatus Step(float dt)
        {
            if (caster?.FightBody?.Casting?.Skill != skill && caster?.FightBody?.CheckSkillCast(skill) == true)
            {
                return BehaviorStatus.SUCCESS;
            }
            else
            {
                return BehaviorStatus.FAILURE;
            }
        }
    }

    public class NpcIsInFightCondition : Condition
    {
        private Npc caster;
        public NpcIsInFightCondition(Npc t) { caster = t; }
        public override void Enter() { }
        public override void Leave(BehaviorStatus status) { }
        public override BehaviorStatus Step(float dt)
        {
            if (caster?.FightBody?.IsFighting == true)
            {
                return BehaviorStatus.SUCCESS;
            }
            else
            {
                return BehaviorStatus.FAILURE;
            }
        }
    }

}
