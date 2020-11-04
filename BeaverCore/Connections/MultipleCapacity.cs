using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Connections
{
    public class MultipleFastenerCapacity
    {
        List<SingleFastenerCapacity> fastener_Cap;
        bool type; //0 for overall connection capacity, 1 for single fastener capacity

        public MultipleFastenerCapacity(List<SingleFastenerCapacity> fastener_cap)
        {
            fastener_Cap = fastener_cap;
        }

        public static double OverallResistance() {

            double result = 0;



            return result;
        
        }

        public List<double> IndividualResistance() {
            List<double> result = new List<double>();

            return result;
        }
       
    }
}
