using System;
using System.Collections.Generic;
using System.Text;

using BeaverCore.Geometry;
using BeaverCore.Actions;

namespace BeaverCore.Connections
{
   public class Connection
    {
        public Dictionary<int, fastInfo> fastPositions;  // fastener index and info

        public struct fastInfo
        {
            public Point3D pt;
            public Plane plane;
            public Force force;
        }
    }
}
