using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BeaverCore.Actions
{
    /// <summary>
    /// Class for individual Force
    /// </summary>
    public class Force: Action
    {
        public double N;
        public double Vy;
        public double Vz;
        public double Mt;
        public double My;
        public double Mz;
        

        public Force() { }

        public Force(double n, double my, double mz, double vy, double vz, double mt, string type)
        {
            N = n;
            Vy = vy;
            Vz = vz;
            Mt = mt;
            My = my;
            Mz = mz;
            this.type = type;
            duration = new TypeInfo(type).duration;
        }

        private Force(List<double> InternalForces)
        {
            N =InternalForces[0];
            Vy =InternalForces[1];
            Vz =InternalForces[2];
            Mt = InternalForces[3];
            My = InternalForces[4];
            Mz = InternalForces[5];
        }

        public List<double> ToList()
        {
            List<double> InternalForces = new List<double>() { 
                N,
                Vy,
                Vz,
                Mt,
                My,
                Mz
            };
            return InternalForces;
        }

        public static Force operator +(Force f1, Force f2)
        {
            List<double> f1List = f1.ToList();
            List<double> f2List = f2.ToList();
            List<double> result = new List<double>();
            foreach (var f in f1List.Zip(f2List, Tuple.Create))
            {
                result.Add(f.Item1 + f.Item2);
            }
            return new Force(result);
        }

        public static Force operator *(double s, Force f1)
        {
            List<double> result = new List<double>();
            List<double> f1List = f1.ToList();
            foreach (var f in f1List)
            {
                result.Add(s * f);
            }
            return new Force(result);
        }

        public static Force operator *(Force f1, double s)
        {
            List<double> result = new List<double>();
            List<double> f1List = f1.ToList();
            foreach (var f in f1List)
            {
                result.Add(s * f);
            }
            return new Force(result);
        }

        /// <summary>
        /// Tests if Internal Forces have same direction
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static bool IsSameDirection(Force f1, Force f2)
        {
            bool result = true;
            List<double> f1List = f1.ToList();
            List<double> f2List = f2.ToList();
            foreach (var f in f1List.Zip(f2List, Tuple.Create))
            {
                if (f.Item1 * f.Item2 < 0)
                {
                    result = false;
                }
            }
            return result;
        }

    }
}
