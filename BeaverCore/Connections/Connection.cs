using System;
using System.Collections.Generic;
using System.Text;

using BeaverCore.Geometry;
using BeaverCore.Actions;
using BeaverCore.Materials;

namespace BeaverCore.Connections
{
   public class Connection
    {
        public SingleFastenerCapacity fastenerCapacity;
        public Fastener fastener;
        public Force force;
        public List<FastData> FastenerList = new List<FastData>();  // fastener index and info
        public Plane plane;
        public Tuple<string, double> critical_connection_utilization;
        public double translationalStiffness = 0;
        public double rotationalStiffness = 0;

        public class FastData
        {
            public FastData(Point2D pt) { this.pt = pt; }

            public Point2D pt;
            public Dictionary<string, double> utilization = new Dictionary<string, double>() {
                {"N",0 },
                {"Vz",0 },
                {"Vy",0 },
                {"Shear",0 },
                {"Axial",0 },
                {"Combined",0 },
            };
            public Tuple<string, double> critical_utilization;
            public Dictionary<string, Vector3D> forces = new Dictionary<string, Vector3D>() {
                {"Faxd", null },
                {"Fvd",  null },
                {"Fx",   null },
                {"Fy",   null },
                {"Fz",   null },
                {"Fi_My",null },
                {"Fi_Mt",null }
            };
        }
    }
}
