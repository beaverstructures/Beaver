using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BeaverCore.Connections;
using BeaverCore.Misc;

namespace BeaverGrasshopper
{
    public class GH_ShearSpacing : GH_Goo<ShearSpacing>
    {
        public GH_ShearSpacing()
        {
            Value = new ShearSpacing();
        }

        public GH_ShearSpacing(ShearSpacing Spacing)
        {
            Value = Spacing.DeepClone<ShearSpacing>();
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
                return "Spacing";
            }
        }

        public override string TypeName
        {
            get
            {
                return "Spacing";
            }
        }

        public override IGH_Goo Duplicate()
        {
            GH_ShearSpacing duplicate = new GH_ShearSpacing();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = "Spacing";
            return info_string;
        }
    }

    public class Param_ShearSpacing : GH_Param<GH_ShearSpacing>
    {
        public Param_ShearSpacing() : base("Spacing", "Spacing", "Fastener Spacing", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Spacing;
            }
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("37958676-b5c1-44c0-baf1-9666ed25cb00");
            }
        }
    }
}