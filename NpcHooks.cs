using Harmony;
using XiaWorld;

namespace BetterDriver
{
    // remove the obsoleted NpcDriver class to make older saves compatible. 
    [HarmonyPatch(typeof(Npc), "OnAfterLoad")]
    internal static class NpcOnAfterLoad
    {
        public static void Postfix(Npc __instance)
        {
            __instance.RemoveStep<NpcDriver>();
        }
    }
}
