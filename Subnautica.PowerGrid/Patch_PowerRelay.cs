using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;

namespace Subnautica.PowerGrid
{
    [HarmonyPatch(typeof(PowerRelay))]
    [HarmonyPatch("IsValidRelayForConnection")]
    internal class Patch_Relay_IsValidRelay
    {
        [Harmony]
        public static void Postfix(PowerRelay potentialRelay, bool includeGhostModels, ref bool __result, PowerRelay __instance)
        {
            if (!__result) return;

            if (potentialRelay is SecondaryRelay)
            {
                __result = false;
                return;
            }

            if (__instance.gameObject == potentialRelay.gameObject)
            {
                __result = false;
                return;
            }

            if (potentialRelay != __instance.outboundRelay && PowerNetworkController.AreInSameNetwork(__instance, potentialRelay))
            {
                __result = false;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(PowerRelay))]
    [HarmonyPatch("DisconnectFromRelay")]
    internal class Patch_Relay_Disconnect
    {
        [HarmonyPrefix]
        public static bool Prefix(PowerRelay __instance, ref PowerRelay __state)
        {
            __state = __instance.outboundRelay;
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(PowerRelay __instance, PowerRelay __state)
        {
            if (__state != null)
            {
                Util.Log(string.Format("Disconnect {0} from {1}", __instance.Describe(), __instance.outboundRelay.Describe()));
                PowerNetworkController.DisconnectRelays(__instance, __instance.outboundRelay);
            }
        }
    }

    [HarmonyPatch(typeof(PowerRelay))]
    [HarmonyPatch("OnDestroy")]
    internal class Patch_Relay_OnDestroy
    {
        [HarmonyPrefix]
        public static bool Prefix(PowerRelay __instance)
        {
            Util.Log("Destroying relay: " + __instance.Describe());
            if (__instance.outboundRelay != null)
            {
                __instance.outboundRelay = null;
                PowerNetworkController.DisconnectRelays(__instance, __instance.outboundRelay);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PowerRelay))]
    [HarmonyPatch("TryConnectToRelay")]
    internal class Patch_Relay_TryConnect
    {

        [HarmonyPostfix]
        public static void Postfix(PowerRelay relay, PowerRelay __instance, ref bool __result)
        {
            if (!__result) return;
            if (relay == null) return;

            Util.Log(string.Format("Connect from {0} to {1}", __instance.Describe(), relay.Describe()));
            PowerNetworkController.ConnectRelays(__instance, relay);

            if (__instance.internalPowerSource != null)
            {
                return;
            }

            if (__instance is SecondaryRelay)
            {
                return;
            }

            if (__instance.gameObject.GetFullName().Contains("/PowerTransmitter")
                && __instance.GetSecondary() == null)
            {
                SecondaryRelay secondary = __instance.gameObject.AddComponent<SecondaryRelay>();
                secondary.maxOutboundDistance = __instance.maxOutboundDistance;
                secondary.dontConnectToRelays = __instance.dontConnectToRelays;
                secondary.AddInboundPower(__instance);

                SecondaryPowerFX secondaryPowerFX = __instance.gameObject.AddComponent<SecondaryPowerFX>();
                secondaryPowerFX.attachPoint = __instance.powerFX.attachPoint;
                secondaryPowerFX.vfxPrefab = __instance.powerFX.vfxPrefab;
                secondary.powerFX = secondaryPowerFX;
            }
        }

        private class SecondaryPowerFX : PowerFX { }
    }

}
