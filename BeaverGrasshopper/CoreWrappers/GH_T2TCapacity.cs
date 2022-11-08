using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BeaverCore.Connections;
using BeaverCore.Misc;

namespace BeaverGrasshopper
{
    public class GH_T2TCapacity : GH_Goo<T2TCapacity>
    {
        public GH_T2TCapacity()
        {
            Value = new T2TCapacity();
        }

        public GH_T2TCapacity(T2TCapacity T2TCapacity)
        {
            Value = T2TCapacity.DeepClone<T2TCapacity>();
        }

        public override bool IsValid
        {
            get
            {
                return true;
            }
        }

        public override string TypeDescription
        {
            get
            {
                return "Cross Section to procede";
            }
        }

        public override string TypeName
        {
            get
            {
                return "Cross Section";
            }
        }

        public override IGH_Goo Duplicate()
        {
            GH_T2TCapacity duplicate = new GH_T2TCapacity();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = "T2TCapacity ";
            return info_string;
        }
    }

    public class Param_T2TCapacity : GH_Param<GH_T2TCapacity>
    {
        public Param_T2TCapacity() : base("T2TCapacity", "SConnection", "T2TCapacity", "Beaver", "0. Parameters", GH_ParamAccess.item) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Dowel;
            }
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("13062c3f-a229-4f30-927a-aa76030466f4");
            }
        }
    }
}