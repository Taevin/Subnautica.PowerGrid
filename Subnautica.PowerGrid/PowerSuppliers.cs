using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Subnautica.PowerGrid
{
    class PowerSuppliers
    {
        public static PowerSuppliers EMPTY = new PowerSuppliers();

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

        private IEnumerable<IPowerInterface> GetLocalAndRemoteSources(PowerRelay relay)
        {
            return _suppliers.Cast<IPowerInterface>()
                .Concat(relay.GetInboundNonRelaySources());
        }
        
    }
}
