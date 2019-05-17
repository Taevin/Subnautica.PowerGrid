using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;

namespace Subnautica.PowerGrid
{
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
