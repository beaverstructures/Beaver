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
    public class GH_ShearConnection : GH_Goo<ShearConnection>
    {
        public GH_ShearConnection()
        {
            Value = new ShearConnection();
        }

        public GH_ShearConnection(ShearConnection connection)
        {
            Value = connection.DeepClone<ShearConnection>();
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
            GH_ShearConnection duplicate = new GH_ShearConnection();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = "ShearConnection ";
            return info_string;
        }
    }

    public class Param_ShearConnection : GH_Param<GH_CroSec>
    {
        public Param_ShearConnection() : base("ShearConnection", "SConnection", "ShearConnection", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.ShearConnection;
            }
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("5031119a-7bde-4329-8cf8-d385f5d7546b");
            }
        }
    }
}