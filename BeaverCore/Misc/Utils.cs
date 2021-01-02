using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaverCore.Misc
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

        static public double KMOD(int SC, string duration)
        {
            double k = 0;
            if (SC == 1 || SC == 2)
            {
                if (duration == "perm")
                {
                    k = 0.6;
                }
                else if (duration == "long")
                {
                    k = 0.7;
                }
                else if (duration == "medium")
                {
                    k = 0.8;
                }
                else if (duration == "short")
                {
                    k = 0.9;
                }
                else if (duration == "inst")
                {
                    k = 1.1;
                }
            }
            else if (SC == 3)
            {
                if (duration == "perm")
                {
                    k = 0.5;
                }
                else if (duration == "long")
                {
                    k = 0.55;
                }
                else if (duration == "medium")
                {
                    k = 0.65;
                }
                else if (duration == "short")
                {
                    k = 0.7;
                }
                else if (duration == "inst")
                {
                    k = 0.9;
                }
            }
            else {throw new ArgumentException( "Service Class must be a integer between 1 and 3"); }
            return k;
        }

        // taken from http://www.interact-sw.co.uk/iangblog/2010/08/01/linq-cartesian-3
        // performs the n-ary cartesian product of many sets
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> inputs)
        {
            return inputs.Aggregate(
                (IEnumerable<IEnumerable<T>>)new T[][] { new T[0] },
                (soFar, input) =>
                    from prevProductItem in soFar
                    from item in input
                    select prevProductItem.Concat(new T[] { item }));
        }

        // Enable variable argument list.
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(
            params IEnumerable<T>[] inputs)
        {
            IEnumerable<IEnumerable<T>> e = inputs;
            return CartesianProduct(e);
        }
    }
}
