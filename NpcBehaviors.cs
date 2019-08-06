using System;
using XiaWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    
    public class NpcCastSkillAction : Action
    {
        private IBlackBoard bb;
        private string skill;
        public NpcCastSkillAction(IBlackBoard blackBoard, string s)
        {
            bb = blackBoard ?? throw new ArgumentNullException("blackBoard");
            skill = s;
        }
        public override void Step(float dt)
        {
            var caster = bb.Get<Npc>("Caster");
            caster.CastSkill(skill, caster.FightBody?.Target, 0);
            status = NodeStatus.SUCCESS;
        }
    }

    public class NpcCanCastSkillCondition : Condition
    {
        private IBlackBoard bb;
        private string skill;
        public NpcCanCastSkillCondition(IBlackBoard blackBoard, string s)
        {
            bb = blackBoard ?? throw new ArgumentNullException("blackBoard");
            skill = s;
        }
        public override void Step(float dt)
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
        private IBlackBoard bb;
        public NpcIsInFightCondition(IBlackBoard blackBoard)
        {
            bb = blackBoard ?? throw new ArgumentNullException("blackBoard");
        }
        public override void Step(float dt)
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
