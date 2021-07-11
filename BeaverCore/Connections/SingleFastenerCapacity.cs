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
        public Dictionary<string, double> shear_capacities;
        public double shear_crictical_capacity;
        public string shear_critical_failure_mode;

        public Dictionary<string, double> axial_capacities;
        public double axial_crictical_capacity;
        public string axial_critical_failure_mode;
        public bool rope_effect;

        public string analysisType;

        public void GetFvk()
        {
            if (sheartype == 1) shear_capacities = FvkSingleShear();
            else if (sheartype == 2) shear_capacities = FvkDoubleShear();
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
            /// Finds and updates the critical capacity from the shear_capacities variable
            shear_crictical_capacity = 9999999;
            axial_crictical_capacity = 9999999;
            foreach (var keyValuePair in shear_capacities)
            {
                if (keyValuePair.Value < shear_crictical_capacity)
                {
                    shear_crictical_capacity = keyValuePair.Value;
                    shear_critical_failure_mode = keyValuePair.Key;
                }
            }
            foreach (var keyValuePair in axial_capacities)
            {
                if (keyValuePair.Value < axial_crictical_capacity)
                {
                    axial_crictical_capacity = keyValuePair.Value;
                    axial_critical_failure_mode = keyValuePair.Key;
                }
            }
        }
    }
}