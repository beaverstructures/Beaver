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
    public class GH_ConnectionAxial : GH_Goo<ConnectionAxial>
    {
        public GH_ConnectionAxial()
        {
            Value = new ConnectionAxial();
        }

        public GH_ConnectionAxial(ConnectionAxial connection)
        {
            Value = connection.DeepClone<ConnectionAxial>();
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
            GH_ConnectionAxial duplicate = new GH_ConnectionAxial();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = "ConnectionAxial ";
            return info_string;
        }
    }

    public class Param_ConnectionAxial : GH_Param<GH_CroSec>
    {
        public Param_ConnectionAxial() : base("ConnectionAxial", "SConnection", "ConnectionAxial", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.AxialConnection;
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