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
    public class GH_TimberFrame : GH_Goo<TimberFrame>
    {
        public GH_TimberFrame()
        {
            Value = new TimberFrame();
        }

        public GH_TimberFrame(TimberFrame frame)
        {
            Value = frame.DeepClone<TimberFrame>();
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
            GH_TimberFrame duplicate = new GH_TimberFrame();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = "TimberFramePoint (" + Value.id + ")";
            return info_string;
         }
    }

    public class Param_TimberFrame : GH_Param<GH_TimberFrame>
    {
        public Param_TimberFrame() : base("TimberFrame", "TFrame", "TimberFrame to procede", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

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
                return new Guid("b1ba4faa-82b3-48fd-b7b0-f34160dcff0a");
            }
        }
    }
}



