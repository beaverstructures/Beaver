using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Actions
{
    [Serializable]
    public class Displacement : Action
    /// <summary>
    /// Displacement class for SLS calculations.
    /// Defines x, y and z displacements as well as load types.
    /// </summary>
    {
        public double dx = 0;    // Global X deflection
        public double dy = 0;    // Global Y deflection
        public double dz = 0;    // Global Z deflection

        // INSTANTIATE CLASS
        public Displacement()
        {
            type = "QX";
            typeinfo = new TypeInfo("QX");
            duration = typeinfo.duration;
            combination = "";
        }

        public Displacement(string type)
        {
            this.type = type;
            typeinfo = new TypeInfo(type);
            duration = typeinfo.duration;
            combination = type;
        }

        public Displacement(double disp, string type)
        {
            dz = disp;
            dx = 0;
            dy = 0;
            this.type = type;
            typeinfo = new TypeInfo(type);
            duration = typeinfo.duration;
            combination = type;
        }

        public Displacement(double dispX, double dispY, double dispZ, string type)
        {
            dx = dispX;
            dy = dispY;
            dz = dispZ;
            this.type = type;
            typeinfo = new TypeInfo(type);
            duration = typeinfo.duration;
            combination = type;
        }

        public static Displacement operator +(Displacement w1, Displacement w2)
        {
            Displacement result = new Displacement(w1.dx + w2.dx,
                                                    w1.dy + w2.dy,
                                                    w1.dz + w2.dz,
                                                    w1.type);
            if (w1.combination == "") { result.combination = w2.combination;
            }
            else if (w2.combination == "") { result.combination = w1.combination;
            }
            else { result.combination = w1.combination + "+" + w2.combination;

            }
            return result;
        }

        public static Displacement operator *(Displacement w1, double s)
        {
            Displacement result = new Displacement(w1.dx * s,
                                                    w1.dy * s,
                                                    w1.dz * s,
                                                    w1.type);
            result.combination = Math.Round(s, 2).ToString() + w1.combination;
            return result;
        }

        public static Displacement operator *(double s, Displacement w1)
        {
            Displacement result = new Displacement(w1.dx * s,
                                                    w1.dy * s,
                                                    w1.dz * s,
                                                    w1.type);
            result.combination = Math.Round(s, 2).ToString() + w1.combination;
            return result;
        }

        public double Absolute()
        {
            return Math.Sqrt(dx*dx + dy*dy + dz*dz);
        }
    }
}
