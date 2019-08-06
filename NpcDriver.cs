using XiaWorld;
using XiaWorld.ThingStep;
using Newtonsoft.Json;
using System.Linq;
using XiaWorld.Fight;

namespace BetterDriver
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [System.Obsolete]
    public class NpcDriver : ThingStep<Npc>
    {
        private BehaviorTree tree;
        public override void OnAfterLoad(Npc t)
        {
            OnEnter(t);
        }

        public override void OnEnter(Npc t, params object[] objs)
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
                    .OrderByDescending( key =>
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
        }

        public override void OnLeave(Npc t, params object[] objs)
        {
            tree?.Leave(NodeStatus.SUCCESS);
        }

        public override ThingStepRes OnStep(Npc t, float dt)
        {
            if (t?.IsDeath == true)
            {
                return ThingStepRes.emDestroySelf;
            }
            else
            {
                tree?.Step(dt);
                return ThingStepRes.emContinue;
            }
        }
    }
}
