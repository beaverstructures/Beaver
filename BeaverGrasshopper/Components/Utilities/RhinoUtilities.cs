using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using bv = BeaverCore.Geometry;
using BeaverCore.Materials;
using BeaverCore.Frame;
using BeaverCore.Actions;

using rh = Rhino.Geometry;

using Karamba.Geometry;
using Karamba.CrossSections;

namespace BeaverGrasshopper.Components.Utilities
{
    public static class RhinoUtilities
    {
        public static bv.Point3D RhinoPt2Beaver(this rh.Point3d rhino_point)
        {
            return new bv.Point3D(rhino_point.X, rhino_point.Y, rhino_point.Z);
        }

        public static bv.Vector3D RhinoVect2Beaver(this rh.Vector3d rhino_vect)
        {
            return new bv.Vector3D(rhino_vect.X, rhino_vect.Y, rhino_vect.Z);
        }

        public static bv.Plane RhinoPlane2Beaver(this rh.Plane rh_plane)
        {
            return new bv.Plane(
                rh_plane.Origin.RhinoPt2Beaver(),
                rh_plane.XAxis.RhinoVect2Beaver(),
                rh_plane.YAxis.RhinoVect2Beaver());
        }

    }
}
