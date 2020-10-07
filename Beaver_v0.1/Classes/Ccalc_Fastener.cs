using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaver_v0._1
{
    public struct Ccalc_Fastener
    {
        public double d;
        public double dh;
        public double l;
        public double fu;
        public string type;
        public bool smooth;

        public Ccalc_Fastener(string fastenerType, double D, double Dh, double L, bool Smooth, double Fu)
        {
            d = D;
            dh = Dh;
            l = L;
            fu = Fu;
            type = fastenerType;
            smooth = Smooth;
        }
    }
}
