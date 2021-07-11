using BeaverCore.Misc;
using System;
using System.Collections.Generic;
using System.Text;
using BeaverCore.Materials;

namespace BeaverCore.CrossSection
{
    [Serializable]
    public class CroSec_Rect : CroSec
    {
        public double b;
        public double h;
        public CroSec_Rect(double height, double width,Material material)
        {
            b = width;
            h = height;
            A = b * h;
            Iy = b * Math.Pow(h, 3) / 12;
            Iz = h * Math.Pow(b, 3) / 12;
            It = GetIt();
            Wy = Iy * (2 / h);
            Wz = Iz * (2 / b);
            ry = Math.Sqrt(Iy / A);
            rz = Math.Sqrt(Iz / A);
            this.material = material;
        }

        public double GetIt()
        {
            List<double> lbeta = new List<double>() { 0.141, 0.196, 0.229, 0.249, 0.263, 0.281, 0.291, 0.299, 0.312, 0.333 };
            List<double> lratio = new List<double>() { 1, 1.5, 2, 2.5, 3, 4, 5, 6, 10, Math.Pow(10, 100) };
            double a = Math.Min(this.b, h);
            double b = Math.Max(this.b, h);
            double ratio = Math.Min(this.b / h, h / this.b);
            double beta = Utils.linear(ratio, lratio, lbeta);
            return beta * a * Math.Pow(b, 3);
        }

        public override double GetsigMcrit(double lef, double E05, double G05)
        {
            return (Math.PI) * Math.Sqrt(E05 * Iz * G05 * It);
        }
    }
}
