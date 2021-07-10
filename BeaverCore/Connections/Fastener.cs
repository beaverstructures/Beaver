using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeaverCore.Geometry;

namespace BeaverCore.Connections
{
    public class Fastener
    {
        public double d;
        public double dh;
        public double l;
        public double fu;
        public string type;
        public bool smooth;

        public Fastener() { }

        public Fastener(string fastenerType, double D, double Dh, double L, bool Smooth, double Fu)
        {
            d = D;
            dh = Dh;
            l = L;
            fu = Fu;
            type = fastenerType;
            smooth = Smooth;
        }
    }

    public class FastenerForce
    {
        public double f = 0;
        public Vector2D force_vector;
        public double alpha = 0;

        public FastenerForce(double f, double alpha)
        {
            this.f = f;
            this.alpha = alpha;
        }

        public FastenerForce(double f, double alpha,Vector2D force_vector)
        {
            this.f = f;
            this.alpha = alpha;
            this.force_vector = force_vector;
        }
    }
}
