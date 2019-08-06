using Harmony;
using XiaWorld.JsonConver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    [HarmonyPatch(typeof(ClassHelperConverter), "GetType", new Type[] { typeof(string) })]
    internal static class JsonPatch
    {
        //because they used full name to save but assembly qualified name to load, I have to patch.
        static void Prefix (ref string t)
        {
            if (t == typeof(NpcDriver).FullName)
            {
                t = typeof(NpcDriver).AssemblyQualifiedName;
            }
        }
    }
}
