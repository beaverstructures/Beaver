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
    public class GH_ConnectionMoment : GH_Goo<ConnectionMoment>
    {
        public GH_ConnectionMoment()
        {
            Value = new ConnectionMoment();
        }

        public GH_ConnectionMoment(ConnectionMoment connection)
        {
            Value = connection.DeepClone<ConnectionMoment>();
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
                return "Beaver Connection element";
            }
        }

        public override string TypeName
        {
            get
            {
                return "Beaver Connection";
            }
        }

        public override IGH_Goo Duplicate()
        {
            GH_ConnectionMoment duplicate = new GH_ConnectionMoment();
            duplicate.Value = Value.DeepClone();
            return duplicate;
        }

        public override string ToString()
        {
            string info_string = "ConnectionMoment ";
            return info_string;
        }
    }

    public class Param_ConnectionMoment : GH_Param<GH_CroSec>
    {
        public Param_ConnectionMoment() : base("ConnectionMoment", "SConnection", "ConnectionMoment", "Beaver", "0.Parameters", GH_ParamAccess.item) { }

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
                return new Guid("5283e7c2-d891-47fa-a11d-4982bdb14368");
            }
        }
    }
}