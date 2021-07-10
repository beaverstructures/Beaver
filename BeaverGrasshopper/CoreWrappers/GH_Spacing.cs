using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BeaverCore.Connections;
using BeaverCore.Misc;

namespace BeaverGrasshopper
{
    public class GH_Spacing : GH_Goo<Spacing>
    {
        public GH_Spacing()
        {
            Value = new Spacing();
        }

        public GH_Spacing(Spacing Spacing)
        {
            Value = Spacing.DeepClone<Spacing>();
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
            GH_Spacing duplicate = new GH_Spacing();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = "Spacing";
            return info_string;
        }
    }

    public class Param_Spacing : GH_Param<GH_Spacing>
    {
        public Param_Spacing() : base("Spacing", "Spacing", "Fastener Spacing", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("48e69e47-be77-41b5-8230-6821f144090c");
            }
        }
    }
}
