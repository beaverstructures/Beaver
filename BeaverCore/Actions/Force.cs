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
    [Serializable]
    public class Force: Action
    {
        public double N=0;
        public double Vy=0;
        public double Vz=0;
        public double Mt=0;
        public double My=0;
        public double Mz=0;
        

        public Force() {
            N = 0;
            Vy = 0;
            Vz = 0;
            Mt = 0;
            My = 0;
            Mz = 0;
            type = "QX";
            typeinfo = new TypeInfo(type);
            duration = typeinfo.duration;
            combination = "";
        }

        public Force(double n,  double vy, double vz, double mt, double my, double mz, string type)
        {
            N = n;
            Vy = vy;
            Vz = vz;
            Mt = mt;
            My = my;
            Mz = mz;
            this.type = type;
            typeinfo = new TypeInfo(type);
            duration = typeinfo.duration;
            combination = type;
        }

        public Force(List<double> InternalForces, string type)
        {
            N =InternalForces[0];
            Vy =InternalForces[1];
            Vz =InternalForces[2];
            Mt = InternalForces[3];
            My = InternalForces[4];
            Mz = InternalForces[5];
            this.type = type;
            typeinfo = new TypeInfo(type);
            duration = typeinfo.duration;
            combination = type;
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
            List<double> resultvalues = new List<double>();
            foreach (var f in f1List.Zip(f2List, Tuple.Create))
            {
                resultvalues.Add(f.Item1 + f.Item2);
            }
            Force result = new Force(resultvalues, f1.type);
            if (f1.combination == "") result.combination = f2.combination;
            else if (f2.combination == "") result.combination = f1.combination;
            else result.combination = f1.combination + "+" + f2.combination;
            return result;
        }

        public static Force operator *(double s, Force f1)
        {
            List<double> resultvalues = new List<double>();
            List<double> f1List = f1.ToList();
            foreach (var f in f1List)
            {
                resultvalues.Add(s * f);
            }
            Force result = new Force(resultvalues, f1.type);
            result.combination = Math.Round(s, 2).ToString() + f1.combination;
            return result;
        }

        public static Force operator *(Force f1, double s)
        {
            List<double> resultvalues = new List<double>();
            List<double> f1List = f1.ToList();
            foreach (var f in f1List)
            {
                resultvalues.Add(s * f);
            }
            Force result = new Force(resultvalues, f1.type);
            result.combination = Math.Round(s, 2).ToString() + f1.combination;
            return result;
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
