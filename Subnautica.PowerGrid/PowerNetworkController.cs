using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using System.Reflection;
using System.Text;

namespace Subnautica.PowerGrid
{
    class PowerNetworkController
    {
        private static Dictionary<String, PowerNetwork> _relayToNetwork = new Dictionary<string, PowerNetwork>();

        public static void ConnectRelays(PowerRelay supplier, PowerRelay consumer)
        {
            if (consumer == null) return;

            PowerNetwork supplierNetwork = GetNetworkByRelay(supplier);
            PowerNetwork consumerNetwork = GetNetworkByRelay(consumer);

            if (supplierNetwork == consumerNetwork && supplierNetwork != null) return;

            string supplierID = supplier.RelayID();
            string consumerID = consumer.RelayID();

            if (supplierNetwork == null && consumerNetwork == null)
            {
                PowerNetwork newNetwork = PowerNetwork.Create();
                AddRelayToNetwork(consumerID, newNetwork);
                AddRelayToNetwork(supplierID, newNetwork);
            }
            else if (supplierNetwork == null)
            {
                AddRelayToNetwork(supplierID, consumerNetwork);
            }
            else if (consumerNetwork == null)
            {
                AddRelayToNetwork(consumerID, supplierNetwork);
            }
            else if (supplierNetwork.Size > consumerNetwork.Size)
            {
                MergeNetworkIntoParent(supplierNetwork, consumerNetwork);
            }
            else
            {
                MergeNetworkIntoParent(consumerNetwork, supplierNetwork);
            }
        }

        public static void DisconnectRelays(PowerRelay supplier, PowerRelay consumer)
        {
            if (consumer == null) return;

            PowerNetwork supplierNetwork = GetNetworkByRelay(supplier);
            PowerNetwork consumerNetwork = GetNetworkByRelay(consumer);

            if (supplierNetwork != consumerNetwork) return;
            if (supplierNetwork == null)
            {
                BuildNewNetwork(GetConnectedRelays(supplier));
                BuildNewNetwork(GetConnectedRelays(consumer));
            }
            else
            {
                BuildNewNetwork(GetConnectedRelays(consumer));
            }
        }

        public static bool AreInSameNetwork(PowerRelay a, PowerRelay b)
        {
            PowerNetwork aNet = GetNetworkByRelay(a);
            PowerNetwork bNet = GetNetworkByRelay(b);
            return aNet == bNet && aNet != null;
        }

        private static void AddRelayToNetwork(string relayID, PowerNetwork network)
        {
            Util.Log(string.Format("Adding relay {0} to network {1}", relayID, network));
            Utils.Assert(_relayToNetwork.GetOrDefault(relayID, null) == null, "Relay already in a network!");
            _relayToNetwork[relayID] = network;
            network.Add(relayID);
        }
        private static void RemoveRelayFromNetwork(string relayID)
        {
            PowerNetwork network = _relayToNetwork.GetOrDefault(relayID, null);
            Util.Log(string.Format("Removing relay {0} from network {1}", relayID, network));
            if (network == null) return;
            network.Remove(relayID);
            _relayToNetwork.Remove(relayID);
        }

        private static void MergeNetworkIntoParent(PowerNetwork parent, PowerNetwork child)
        {
            Util.Log(string.Format("Merging network {0} into network {1}", child, parent));
            foreach (String id in child.RelayIDs)
            {
                RemoveRelayFromNetwork(id);
                AddRelayToNetwork(id, parent);
            }
        }

        private static List<string> GetConnectedRelays(PowerRelay relay)
        {
            return relay.EnumerateConnections()
                .Select(RelayExtensions.RelayID)
                .ToList();
        }

        private static void BuildNewNetwork(List<string> relayIDs)
        {
            PowerNetwork newNetwork = PowerNetwork.Create();
            Util.Log(string.Format("Creating network {0} for {1} relays", newNetwork, relayIDs.Count));
            foreach (string id in relayIDs)
            {
                RemoveRelayFromNetwork(id);
                AddRelayToNetwork(id, newNetwork);
            }
        }


        private static PowerNetwork GetNetworkByRelay(PowerRelay relay) => relay == null ? null : _relayToNetwork.GetOrDefault(relay.RelayID(), null);


        private class PowerNetwork
        {
            private HashSet<string> _members = new HashSet<string>();

            private PowerNetwork(string id)
            {
                this.ID = id;
            }

            public string ID { get; }

            public void Add(PowerRelay relay)
            {
                Add(relay.RelayID());
            }
            public void Add(string relayID)
            {
                _members.Add(relayID);
            }
            public void Remove(PowerRelay relay)
            {
                Remove(relay.RelayID());
            }
            public void Remove(string relayID)
            {
                _members.Remove(relayID);
                if (_members.Count == 0)
                {
                    _networksByID.Remove(ID);
                }
            }

            public int Size => _members.Count;

            public List<string> RelayIDs => _members.ToList();


            private static Dictionary<String, PowerNetwork> _networksByID = new Dictionary<string, PowerNetwork>();
            private static long _nextNetworkID = 0;

            public static PowerNetwork ByID(string id) => _networksByID.GetOrDefault(id, null);
            public static PowerNetwork Create()
            {
                PowerNetwork newNetwork = new PowerNetwork(GenerateNetworkID());
                _networksByID[newNetwork.ID] = newNetwork;
                return newNetwork;
            }

            private static string GenerateNetworkID()
            {
                string id;
                while (_networksByID.ContainsKey(id = (_nextNetworkID = (_nextNetworkID + 1) % int.MaxValue).ToString())) ;
                return id;
            }

            public override bool Equals(object obj) => obj is PowerNetwork && ID == ((PowerNetwork)obj).ID;
            public override int GetHashCode() => ID.GetHashCode();
            public override string ToString()
            {
                return string.Format("{0} [{1}]", ID, Size);
            }
        }
    }
}
