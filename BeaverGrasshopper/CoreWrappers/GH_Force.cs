using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BeaverCore.Actions;
using BeaverCore.Misc;

namespace BeaverGrasshopper
{
    public class GH_Force : GH_Goo<Force>
    {
        public GH_Force()
        {
            Value = new Force();
        }

        public GH_Force(Force force)
        {
            Value = force.DeepClone<Force>();
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
                return "Force";
            }
        }

        public override string TypeName
        {
            get
            {
                return "Force";
            }
        }

        public override IGH_Goo Duplicate()
        {
            GH_Force duplicate = new GH_Force();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = Value.type;
            return info_string;
        }
    }

    public class Param_Force : GH_Param<GH_Force>
    {
        public Param_Force() : base("Force", "Force", "Nodal Force", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Force;
            }
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("df377244-8aa2-4e1e-810c-6041400d6280");
            }
        }
    }
}