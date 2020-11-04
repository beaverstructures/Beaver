using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Connections
{
    public static class MultipleFastenerCapacity
    {
        List<SingleFastenerCapacity> fastener_Cap;
        bool type; //0 for overall connection capacity, 1 for single fastener capacity

        public MultipleFastenerCapacity(List<SingleFastenerCapacity> fastener_cap)
        {
            fastener_Cap = fastener_cap;
        }

        public static double OverallResistance() {
        
        
        }

        public List<double> IndividualResistance() {

        }
       
    }
}
