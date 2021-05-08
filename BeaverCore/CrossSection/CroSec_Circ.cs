using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.CrossSection
{
    public class CroSec_Circ : CroSec
    {
        public double d;

        public CroSec_Circ() { }

        public CroSec_Circ(double diam)
        {
            d = diam;
            A = Math.PI * Math.Pow(d, 2) / 4;
            Iy = Math.PI * Math.Pow(d, 4) / 64;
            Iz = Iy;
            It = Math.PI * Math.Pow(d, 4) / 32;
            Wy = Iy * (2 / d);
            Wz = Iz * (2 / d);
            ry = Math.Sqrt(Iy / A);
            rz = Math.Sqrt(Iz / A);
        }

        public override double GetsigMcrit(double lef, double E05, double G05)
        {
            return (Math.PI) * Math.Sqrt(E05 * Iz * G05 * It);
        }
    }
}
