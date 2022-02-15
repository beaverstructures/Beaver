using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace BeaverCore.Misc
{

    public class Utils
    {
        /// <summary>
        /// Retrieves interpolated color from a list of colors and a double between range [0-1].
        /// If higher than 1, Color is black.
        /// </summary>
        /// <param name="colors"></param>
        /// <param name=""></param>
        /// <returns></returns>
        static public Color colorInterpolation(List<Color> colors, double value)
        {
            Color interpolatedColor = Color.Black;
            int colorCount = colors.Count;
            double colorInterval = Math.Pow((colorCount - 1),-1);
            double intervalBottom = 0;
            double intervalTop = colorInterval;
            for (int i = 0; i < colorCount - 1; i++)
            {
                if (value>=intervalBottom && value <= intervalTop)
                {
                    Color bottomColor = colors[i];
                    Color topColor = colors[i + 1];
                    int newA = (int)interpolate(value, intervalBottom, intervalTop, bottomColor.A, topColor.A);
                    int newR = (int)interpolate(value, intervalBottom, intervalTop, bottomColor.R, topColor.R);
                    int newG = (int)interpolate(value, intervalBottom, intervalTop, bottomColor.G, topColor.G);
                    int newB = (int)interpolate(value, intervalBottom, intervalTop, bottomColor.B, topColor.B);
                    interpolatedColor = Color.FromArgb(newA, newR, newG, newB);
                    break;
                }
                intervalBottom += colorInterval;
                intervalTop += colorInterval;
            }
            return interpolatedColor;
        }

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

    public static class ExtensionMethods
    {
        // Deep clone
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
