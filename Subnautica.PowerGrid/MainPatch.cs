using System.Reflection;
using Harmony;
using UnityEngine;
using UWE;

namespace Subnautica.PowerGrid
{
    public class MainPatch
    {
        public static void Patch()
        {
            var harmony = HarmonyInstance.Create("net.xensoft.subnautica.powergrid.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
