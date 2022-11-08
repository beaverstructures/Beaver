using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;

namespace BeaverGrasshopper.CoreWrappers
{
    public class BeaverGHSettings : Grasshopper.Kernel.GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("Beaver", Properties.Resources.BeaverIcon);
            Instances.ComponentServer.AddCategorySymbolName("Beaver", 'B');
            return GH_LoadingInstruction.Proceed;
        }
    }
}
