using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaver_v0._1.Classes
{

    class Utils
    {

        static public double linear(double x, List<double> xd, List<double> yd)
        {
            double result = 0;
            for (int i = 0; i < xd.Count-1; i++)
            {
                if (x >= xd[i] && x <= xd[i + 1])
                {
                    result = interpolate(x, xd[i], xd[i + 1], yd[i], yd[i + 1]);
                    return result;
                }
            }
            return result;
        }
            static public double interpolate(double x, double x0, double x1, double y0, double y1)
        {
            if ((x1 - x0) == 0)
            {
                return (y0 + y1) / 2;
            }
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }
    }
}
