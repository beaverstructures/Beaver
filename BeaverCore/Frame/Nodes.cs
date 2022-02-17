using System;
using System.Collections.Generic;
using System.Text;
using BeaverCore.Geometry;

namespace BeaverCore.Frame
{
    public class Node
    {
        public Point3D pt;
        public int index;         // node ID
        public List<int> tfPts;   // IDs of connected FE nodes

        public Node() { }
        public Node(Point3D point,int id, List<int> tfPts = null ) 
        {
            pt = point;
            index = id;
            this.tfPts = tfPts;
        }
    }


}
