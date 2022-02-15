using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BeaverCore.Materials;
using BeaverCore.Misc;

namespace BeaverGrasshopper
{
    public class GH_Material : GH_Goo<Material>
    {
        public GH_Material()
        {
            Value = new Material();
        }

        public GH_Material(Material material)
        {
            Value = material.DeepClone<Material>();
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
                return "Material to procede";
            }
        }

        public override string TypeName
        {
            get
            {
                return "Material";
            }
        }

        public override IGH_Goo Duplicate()
        {
            GH_Material duplicate = new GH_Material();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = "Material (" + Value.name + ")";
            return info_string;
        }
    }

    public class Param_Material : GH_Param<GH_Material>
    {
        public Param_Material() : base("Material", "Material", "Timber Material", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Material;
            }
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("54424f19-abff-490b-9ba7-ba58076561f7");
            }
        }
    }
}