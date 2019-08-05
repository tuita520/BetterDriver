using System.Collections.Generic;
using System.Linq;
using XiaWorld;
using XiaWorld.Fight;

namespace BetterDriver
{
    public class NpcController
    {
        private BehaviorTree tree;
        public void Enter(Npc t)
        {
            var skills = t?.FightBody?.Skills;
            if (skills != null)
            {
                var list = skills.Keys
                    .Where(key =>
                    {
                        var def = FightMgr.Instance.GetSkillDef(key);
                        return key != "NormalAttack" && def.Kind != g_emFightSkillKind.Health && def.Kind != g_emFightSkillKind.Sheild;
                    })
                    .OrderByDescending(key =>
                    {
                        return FightMgr.Instance.GetSkillDef(key).Kind;
                    });
                var builder = new NpcBehaviorBuilder(t);
                tree = builder
                    .Root()
                        .Selector()
                            .Filter()
                                .IsInFightCondition()
                                .Selector()
                                    .CastFilters(list)
                                .End()
                            .End()
                        .Action(new FakeSuccessAction())
                    .Build();
            }
            tree?.Enter();
        }

        public void Step(float dt)
        {
            tree?.Step(dt);
        }
    }
}
