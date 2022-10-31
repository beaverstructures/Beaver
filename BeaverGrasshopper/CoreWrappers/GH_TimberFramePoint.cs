using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BeaverCore.Frame;
using BeaverCore.Misc;

namespace BeaverGrasshopper
{
    public class GH_TimberFramePoint : GH_Goo<TimberFramePoint>
    {
        public GH_TimberFramePoint()
        {
            Value = new TimberFramePoint();
        }

        public GH_TimberFramePoint(TimberFramePoint framepoint)
        {
            Value = framepoint.DeepClone<TimberFramePoint>();
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
                return "TimberFramePoint to procede";
            }
        }

        public override string TypeName
        {
            get
            {
                return "TimberFramePoint";
            }
        }

        public override IGH_Goo Duplicate()
        {
            GH_TimberFramePoint duplicate = new GH_TimberFramePoint();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = "TimberFramePoint (" + Value.id + ")";
            return info_string;
        }
    }

    public class Param_TFPoint : GH_Param<GH_TimberFramePoint>
    {
        public Param_TFPoint() : base("TimberFramePoint", "TFPoint", "TimberFramePoint to procede", "Beaver", "0. Parameters", GH_ParamAccess.item) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.TimberFramePoint;
            }
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("22bd7e35-863f-4948-981a-d8d0247c14a5");
            }
        }
    }
}

