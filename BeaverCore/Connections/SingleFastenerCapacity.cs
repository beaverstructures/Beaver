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
        public int sheartype; //1 for single shear, 2 for double shear
        public Dictionary<string, double> capacities;
        public double critical_capacity;
        public string critical_failure_mode;
        public bool rope_effect;
        public string analysisType;

        public void GetFvk()
        {
            if (sheartype == 1) capacities = FvkSingleShear();
            else if (sheartype == 2) capacities = FvkDoubleShear();
        }

        public abstract Dictionary<string, double> FvkSingleShear();

        public abstract Dictionary<string, double> FvkDoubleShear();

        public abstract Dictionary<string, double> Faxk();

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

        public void SetCriticalCapacity()
        {
            /// Finds and updates the critical capacity from the capacities variable
            critical_capacity = 9999999;
            foreach(var keyValuePair in capacities)
            {
                if (keyValuePair.Value < critical_capacity)
                {
                    critical_capacity = keyValuePair.Value;
                    critical_failure_mode = keyValuePair.Key;
                }
            }
        }
    }
}