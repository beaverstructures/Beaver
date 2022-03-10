using System;
using System.Collections.Generic;
using System.Text;

using BeaverCore.Geometry;
using BeaverCore.Actions;

namespace BeaverCore.Connections
{
   public class Connection
    {
        public Fastener fastener;
        public Force force;
        public List<FastData> FastenerList;  // fastener index and info
        public Plane plane;

        public struct FastData
        {
            public Point2D pt;
            
            public FastenerForce force;
            public double utilization;
            public string critical_force;
        }
    }
}
