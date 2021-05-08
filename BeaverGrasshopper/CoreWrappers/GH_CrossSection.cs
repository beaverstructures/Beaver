using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BeaverCore.CrossSection;
using BeaverCore.Misc;

namespace BeaverGrasshopper
{
    public class GH_CroSec : GH_Goo<CroSec>
    {
        public GH_CroSec()
        {
            Value = new CroSec_Circ();
        }

        public GH_CroSec(CroSec crosec)
        {
            Value = crosec.DeepClone<CroSec>();
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
            GH_CroSec duplicate = new GH_CroSec();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            if (Value is CroSec_Rect)
            {
                CroSec_Rect RectValue = (CroSec_Rect)Value;
                return "CrossSection (h:" + Math.Round(RectValue.h, 2) + "cm   w:" + Math.Round(RectValue.b, 2) + "cm)";
            }
            else if (Value is CroSec_Circ)
            {
                CroSec_Circ CircValue = (CroSec_Circ)Value;
                return "CrossSection (h:" + Math.Round(CircValue.d, 2) + "cm)";
            }
            else
            {
                return "CrossSection";
            }
        }
    }

    public class Param_CroSec : GH_Param<GH_CroSec>
    {
        public Param_CroSec() : base("Cross Section", "CroSec", "Timber Cross Section", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

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
                return new Guid("f50124eb-909e-406b-9dcc-718917b40f36");
            }
        }
    }
}