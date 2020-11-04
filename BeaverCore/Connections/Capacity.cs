using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Connections
{


    public abstract class SingleFastenerCapacity
    {
        public Variables variables;
        public Fastener fastener;
        public double t1;
        public Material tMat1;
        public double alfa1;
        public double alfafast;
        public string timberMaterial;
        public string connectorMaterial;
        public bool preDrilled;
        public double pk1;



        public abstract object FvkSingleShear(bool type);

        public abstract object FvkDoubleShear(bool type);

        public double FaxrkUpperLimitValue()
        {
            string type = fastener.type;

            if (type == "nail")
            {
                return 0.15;
            }

            else if (type == "screw")
            {
                return 1;
            }
            else if (type == "bolt")
            {
                return 0.25;
            }
            else if (type == "dowel")
            {
                return 0;
            }
            return 1;
        }


    }
}