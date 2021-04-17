using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BeaverCore.Actions
{

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

        private Force(Dictionary<string, double> InternalForces)
        {
            N =InternalForces["N"];
            Vy =InternalForces["Vy"];
            Vz =InternalForces["Vz"];
            Mt = InternalForces["Mt"];
            My = InternalForces["My"];
            Mz = InternalForces["Mz"];
        }

        private Dictionary<string, double> toDictionary()
        {
            Dictionary<string, double> InternalForces = new Dictionary<string, double>();
            InternalForces["N"] = N;
            InternalForces["Vy"] = Vy;
            InternalForces["Vz"] = Vz;
            InternalForces["Mt"] = Mt;
            InternalForces["My"] = My;
            InternalForces["Mz"] = Mz;
            return InternalForces;
        }

        public static Force operator +(Force f1, Force f2)
        {
            Force result = new Force();
            Dictionary<string, double>  f1Dict = f1.toDictionary();
            Dictionary<string, double>  f2Dict = f2.toDictionary();
            foreach (KeyValuePair<string, double> f in f1Dict)
            {
                f1Dict[f.Key] += f2Dict[f.Key];
            }
            return new Force(f1Dict);
        }

        public static Force operator *(double s, Force f1)
        {
            Force result = new Force();
            Dictionary<string, double> f1Dict = f1.toDictionary();
            foreach (KeyValuePair<string, double> f in f1Dict)
            {
                f1Dict[f.Key] *= s;
            }
            return new Force(f1Dict);
        }

        public static Force operator *(Force f1, double s)
        {
            Force result = new Force();
            Dictionary<string, double> f1Dict = f1.toDictionary();
            foreach (KeyValuePair<string, double> f in f1Dict)
            {
                f1Dict[f.Key] *= s;
            }
            return new Force(f1Dict);
        }

        public static bool operator *(Force f1, Force f2)
        {
            // Tests if Internal Forces have same direction
            bool result = true;
            Dictionary<string, double> f1Dict = f1.toDictionary();
            Dictionary<string, double> f2Dict = f2.toDictionary();
            foreach (string key in f1Dict.Keys)
            {
                if (f1Dict[key] * f2Dict[key] < 0)
                {
                    result = false;
                }
            }
            return result;
        }

        // Moved TypeInfo class to Action.cs
    }
}
