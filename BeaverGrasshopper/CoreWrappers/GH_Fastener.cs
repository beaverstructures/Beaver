using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BeaverCore.Connections;
using BeaverCore.Misc;

namespace BeaverGrasshopper
{
    public class GH_Fastener : GH_Goo<Fastener>
    {
        public GH_Fastener()
        {
            Value = new Fastener();
        }

        public GH_Fastener(Fastener fastener)
        {
            Value = fastener.DeepClone<Fastener>();
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
                return "Fastener";
            }
        }

        public override string TypeName
        {
            get
            {
                return "Fastener";
            }
        }

        public override IGH_Goo Duplicate()
        {
            GH_Fastener duplicate = new GH_Fastener();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = Value.type;
            return info_string;
        }
    }

    public class Param_Fastener : GH_Param<GH_Fastener>
    {
        public Param_Fastener() : base("Fastener", "Fastener", "Timber fastener", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Bolt;
            }
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("a1f05e10-535e-4028-87e9-e6aa0e66505d");
            }
        }
    }
}
