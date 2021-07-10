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
        public double ds;
        public double dh;
        public double l;
        public double fu;
        public string type;
        public bool smooth;

        public Fastener() { }

        //dowel
        public Fastener(string fastenerType, double D, double L, double Fu)
        {
            d = D;
            ds = D;
            dh = D;
            l = L;
            fu = Fu;
            type = fastenerType;
            smooth = true;
        }

        //bolt
        public Fastener(string fastenerType, double D, double Dh, double L, double Fu)
        {
            d = D;
            ds = D;
            dh = Dh;
            l = L;
            fu = Fu;
            type = fastenerType;
            smooth = true;
        }

        //nail
        public Fastener(string fastenerType, double D, double Dh, double L, bool Smooth, double Fu)
        {
            d = D;
            ds = D;
            dh = Dh;
            l = L;
            fu = Fu;
            type = fastenerType;
            smooth = Smooth;
        }

        //screw
        public Fastener(string fastenerType, double D, double Ds, double Dh, double L, double Fu)
        {
            d = D;
            ds = Ds;
            dh = Dh;
            l = L;
            fu = Fu;
            type = fastenerType;
            smooth = true;
        }

    }

    public class FastenerForce
    {
        public double f = 0;
        public Vector2D force_vector;
        public double alpha = 0;
        public string duration;

        public FastenerForce(double f, double alpha,string duration)
        {
            this.f = f;
            this.alpha = alpha;
            this.duration = duration;
        }

        public FastenerForce(double f, double alpha,Vector2D force_vector,string duration)
        {
            this.f = f;
            this.alpha = alpha;
            this.force_vector = force_vector;
            this.duration = duration;
        }
    }
}
