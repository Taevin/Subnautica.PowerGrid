﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Subnautica.PowerGrid
{
    class PowerSuppliers
    {
        // Dummy supplier for relays not on a network
        public static PowerSuppliers EMPTY = new PowerSuppliers();

        // Map Relay ID to Relay, but only for relays with internal power
        private static Dictionary<string, PowerRelay> _supplierMap = new Dictionary<string, PowerRelay>();

        public static void RegisterRelay(PowerRelay relay)
        {
            if (relay.internalPowerSource != null)
            {
                _supplierMap[relay.RelayID()] = relay;
            }
        }

        // ---------------------------


        private HashSet<PowerRelay> _suppliers = new HashSet<PowerRelay>();

        public void AddRelay(string relayID)
        {
            if (_supplierMap.ContainsKey(relayID))
                _suppliers.Add(_supplierMap[relayID]);
        }

        public void RemoveRelay(string relayID)
        {
            if (_supplierMap.ContainsKey(relayID))
                _suppliers.Remove(_supplierMap[relayID]);
        }

        public void DestroyRelay(PowerRelay relay)
        {
            _supplierMap.Remove(relay.RelayID());
            _suppliers.Remove(relay);
        }

        public float GetMaxPower(PowerRelay sender)
        {
            return GetLocalAndRemoteSources(sender).Sum(a => a.GetMaxPower());
        }
        public float GetPower(PowerRelay sender)
        {
            return GetLocalAndRemoteSources(sender).Sum(a => a.GetPower());
        }

        // This is identical to the original ModifyPower in the game code, except that it uses
        // a different set of sources.
        public bool ModifyPower(PowerRelay sender, float amount, out float modified)
        {
            bool result = false;
            modified = 0f;
            float original = amount;
            foreach (IPowerInterface source in GetLocalAndRemoteSources(sender))
            {
                float num = 0f;
                result = source.ModifyPower(amount, out num);
                modified += num;
                amount -= num;
                if (result)
                    break;
            }

            UWE.Utils.Assert(UWE.Utils.SameSign(original, modified), "ModifyPowerFromInbound amount and result must have same sign", null);
            return result;
        }

        // Only PowerRelays are handled by the network.  When we look at available power sources, we also need to take into account
        // non-relay nodes such as PowerSources, BatterySources, etc.  This returns the PowerRelays on the network, and all non-relay
        // sources connected directly to this node (Such as generators or batteries inside a base).
        private IEnumerable<IPowerInterface> GetLocalAndRemoteSources(PowerRelay relay)
        {
            return _suppliers.Cast<IPowerInterface>()
                .Concat(relay.GetInboundNonRelaySources());
        }
        
    }
}
