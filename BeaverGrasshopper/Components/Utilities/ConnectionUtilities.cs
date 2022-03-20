using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BeaverCore.Actions;
using BeaverCore.Connections;
using BeaverCore.CrossSection;
using BeaverCore.Frame;
using bv = BeaverCore.Geometry;

using Rhino.Geometry;

namespace BeaverGrasshopper.Components.Utilities
{
    public static class ConnectionUtilities
    {
        public static List<Mesh> ConnectionToMesh(TimberFramePoint element1, TimberFramePoint element2, bv.Plane plane, Connection connection)
        {
            // displays the arrangement of timber elements, steel plates and fasteners and CR of connection
            throw new NotImplementedException();
        }

        public static Tuple<Point3d,Vector3d> DisplayForces(Connection connection, SLSCombinations sls, ULSCombinations uls)
        {
            // display acting forces in each fastener for the selected ULS or SLS combination and CR of connection
            throw new NotImplementedException();
        }

        public static List<Mesh> ColorFasteners(Connection connection, SLSCombinations sls ,ULSCombinations uls)
        {
            // colors fastener utilizations based on selected ULS or SLS combination
            throw new NotImplementedException();
        }
    }
}
