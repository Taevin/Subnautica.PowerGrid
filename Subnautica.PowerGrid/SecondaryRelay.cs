using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;

namespace Subnautica.PowerGrid
{
    /// <summary>
    /// Error-catching (But otherwise unchanged) subclass to help distinguish the added relays from the existing ones.
    /// </summary>
    internal class SecondaryRelay : PowerRelay
    {
        public override void Start()
        {
            try
            {
                base.Start();
            }
            catch (Exception ex)
            {
                Util.LogError("Error starting SecondaryRelay", ex);
            }
        }

    }
}
