using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BeaverCore.Actions;
using BeaverCore.Misc;

namespace BeaverGrasshopper
{
    public class GH_Displacement : GH_Goo<Displacement>
    {
        public GH_Displacement()
        {
            Value = new Displacement();
        }

        public GH_Displacement(Displacement displacement)
        {
            Value = displacement.DeepClone<Displacement>();
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
                return "Displacement";
            }
        }

        public override string TypeName
        {
            get
            {
                return "Displacement";
            }
        }

        public override IGH_Goo Duplicate()
        {
            GH_Displacement duplicate = new GH_Displacement();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = Value.type;
            return info_string;
        }
    }

    public class Param_Displacement : GH_Param<GH_Displacement>
    {
        public Param_Displacement() : base("Displacement", "Displacement", "Nodal Displacement", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

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
                return new Guid("89611faf-7fd4-4342-acfb-6b510cd9e350");
            }
        }
    }
}