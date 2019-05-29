using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;

namespace Subnautica.PowerGrid
{
    internal static class RelayExtensions
    {
        private static FieldInfo RELAY_INBOUND_FIELD = AccessTools.Field(typeof(PowerRelay), "inboundPowerSources");
        private static MethodInfo DISCONNECT_FROM_RELAY = AccessTools.Method(typeof(PowerRelay), "DisconnectFromRelay");

        /// <summary>
        /// Find the primary relay on the same GameObject as the specified SecondaryRelay, if any.
        /// </summary>
        public static PowerRelay GetPrimary(this SecondaryRelay secondary)
        {
            PowerRelay[] relays = secondary.gameObject.GetComponents<PowerRelay>();
            foreach (PowerRelay relay in relays)
            {
                if (relay != secondary)
                {
                    return relay;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the secondary relay on the same GameObject as the specified primary PowerRelay, if any.
        /// </summary>
        public static SecondaryRelay GetSecondary(this PowerRelay relay)
        {
            return relay.gameObject.GetComponent<SecondaryRelay>();
        }

        /// <summary>
        /// Given one relay on a GameObject, find the other, if any.
        /// </summary>
        public static PowerRelay GetSibling(this PowerRelay sender)
        {
            PowerRelay[] relays = sender.gameObject.GetComponents<PowerRelay>();
            foreach (PowerRelay relay in relays)
            {
                if (relay != sender)
                {
                    return relay;
                }
            }
            return null;
        }

        /// <summary>
        /// Return all PowerRelays on a GameObject, including the supplied argument.
        /// </summary>
        public static PowerRelay[] GetSiblings(this PowerRelay sender)
        {
            return sender.gameObject.GetComponents<PowerRelay>().ToArray();
        }

        /// <summary>
        /// Get an abbreviated name and GUID to describe this relay, suitable for logging.
        /// </summary>
        public static String Describe(this PowerRelay relay)
        {
            if (relay == null) return "(null)";

            if (relay.gameObject == null) return relay.ToString();

            string type;
            if (relay.ToString().Contains("Transmitter"))
            {
                if (relay is SecondaryRelay)
                    type = "TX-s";
                else
                    type = "TX-p";
            }
            else if (relay.ToString().Contains("Solar"))
                type = "Spwr";
            else if (relay.ToString().Contains("Therm"))
                type = "Thrm";
            else if (relay.ToString().Contains("Base"))
                type = "Base";
            else
                type = relay.ToString();

            return type + " " + relay.gameObject.GetId().Substring(0, 4);
        }

        /// <summary>
        /// Return the ID of a relay's GameObject
        /// </summary>
        public static String RelayID(this PowerRelay relay) => relay.gameObject.GetId();


        public enum Direction
        {
            Both,
            Inbound,
            Outbound
        }

        /// <summary>
        /// Traverse the tree of relays and return all relays connected to the specified one
        /// </summary>
        public static IEnumerable<PowerRelay> EnumerateConnections(this PowerRelay relay, Direction dir = Direction.Both)
        {
            yield return relay;
            if (dir == Direction.Both || dir == Direction.Inbound)
            {
                foreach (PowerRelay supplier in GetInboundRelays(relay))
                    foreach (PowerRelay other in EnumerateConnections(supplier, Direction.Inbound))
                        yield return other;
            }

            if (dir == Direction.Both || dir == Direction.Outbound)
            {
                foreach (PowerRelay sibling in relay.GetComponents<PowerRelay>())
                {
                    if (sibling.outboundRelay != null)
                        foreach (PowerRelay other in EnumerateConnections(sibling.outboundRelay, Direction.Outbound))
                            yield return other;
                }
            }
        }

        /// <summary>
        /// Remove the outbound connection from this relay
        /// </summary>
        public static void DisconnectFromConsumer(this PowerRelay relay)
        {
            DISCONNECT_FROM_RELAY.Invoke(relay, null);
        }

        /// <summary>
        /// Remove all inbound connections from this relay
        /// </summary>
        public static void DisconnectFromPrimary(this SecondaryRelay relay)
        {
            (RELAY_INBOUND_FIELD.GetValue(relay) as List<IPowerInterface>).Clear();
        }

        /// <summary>
        /// Return all inbound sources on this relay that are NOT other relays (PowerSources, BatterySources, etc)
        /// </summary>
        /// <param name="relay"></param>
        public static IEnumerable<IPowerInterface> GetInboundNonRelaySources(this PowerRelay relay)
        {
            return (RELAY_INBOUND_FIELD.GetValue(relay) as List<IPowerInterface>)
                .Where(a => !(a is PowerRelay));
        }

        private static IEnumerable<PowerRelay> GetInboundRelays(PowerRelay relay)
        {
            return (RELAY_INBOUND_FIELD.GetValue(relay) as List<IPowerInterface>)
                .OfType<PowerRelay>();
        }

    }
}
