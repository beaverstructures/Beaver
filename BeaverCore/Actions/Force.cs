using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BeaverCore.Actions
{
    using IF = Dictionary<string, double>;

    public class Force: Action
    {
        public IF InternalForces = new IF() {
        {"N",0},{"Vy",0},{"Vz",0},{"Mt",0},{"My",0},{"Mz",0}
        };
        

        public Force() { }

        public Force(double n, double my, double mz, double vy, double vz, double mt, string type)
        {
            InternalForces["N"] = n;
            InternalForces["Vy"] = vy;
            InternalForces["Vz"] = vz;
            InternalForces["Mt"] = mt;
            InternalForces["My"] = my;
            InternalForces["Mz"] = mz;
            this.type = type;
            duration = new TypeInfo(type).duration;
        }

        public static Force operator +(Force f1, Force f2)
        {
            Force result = new Force();
            foreach (KeyValuePair<string, double> f in f1.InternalForces)
            {
                // $$$ soma os strings?
                result.InternalForces[f.Key] = f.Value + f2.InternalForces[f.Key];
            }
            result.type = f1.type;
            return result;
        }

        public static Force operator *(double s, Force f1)
        {
            Force result = new Force();
            foreach (KeyValuePair<string, double> f in f1.InternalForces)
            {
                result.InternalForces[f.Key] = s * f.Value;
            }
            result.type = f1.type;
            return result;
        }

        public static Force operator *(Force f1, double s)
        {
            Force result = new Force();
            foreach (KeyValuePair<string, double> f in f1.InternalForces)
            {
                result.InternalForces[f.Key] = s * f.Value;
            }
            result.type = f1.type;
            return result;
        }

        public static bool operator *(Force f1, Force f2)
        {
            // Tests if Internal Forces have same direction
            bool result = true;
            foreach (string key in f1.InternalForces.Keys)
            {
                if (f1.InternalForces[key] * f2.InternalForces[key] < 0)
                {
                    result = false;
                }
            }
            return result;
        }

        // Moved TypeInfo class to Action.cs
    }
}
