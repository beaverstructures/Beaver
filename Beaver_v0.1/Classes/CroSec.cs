using Rhino.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Beaver_v0._1.Classes
{
    public abstract class CroSec
    {

        public double A;
        public double Iy;
        public double Iz;
        public double It;


    }

    public class CroSec_Rect : CroSec
    {
        public double b;
        public double h;
        public CroSec_Rect(double height, double width)
        {
            b = width;
            h = height;
            A = b * h;
            Iy = b * Math.Pow(h, 3) / 12;
            Iz = h * Math.Pow(b, 3) / 12;
            It = GetIt();
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
    }

    public class CroSec_Circ : CroSec
    {
        public double d;

        public CroSec_Circ(double diam)
        {
            d = diam;
            A = Math.PI * Math.Pow(d, 2) / 4;
            Iy = Math.PI * Math.Pow(d, 4) / 64;
            Iz = Iy;
            It = Math.PI * Math.Pow(d, 4) / 32;
        }
    }
}
