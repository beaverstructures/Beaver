﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Actions
{
    public class Displacement : Action
    {
        public double dx = 0;    // Global X deflection
        public double dy = 0;    // Global Y deflection
        public double dz = 0;    // Global Z deflection

        // INSTANTIATE CLASS
        public Displacement() { }

        public Displacement(string type)
        {
            this.type = type;
            duration = new TypeInfo(type).duration;
        }

        public Displacement(double disp, string type)
        {
            dz = disp;
            dx = 0;
            dy = 0;
            this.type = type;
            duration = new TypeInfo(type).duration;
        }

        public Displacement(double dispX, double dispY, double dispZ, string type)
        {
            dx = dispX;
            dy = dispY;
            dz = dispZ;
            this.type = type;
            duration = new TypeInfo(type).duration;
        }

        public static Displacement operator +(Displacement w1, Displacement w2)
        {
            Displacement result = new Displacement( w1.dx + w2.dx,
                                                    w1.dy + w2.dy, 
                                                    w1.dz + w2.dz,
                                                    w1.type);
            return result;
        }

        public static Displacement operator *(Displacement w1, double s)
        {
            Displacement result = new Displacement( w1.dx * s,
                                                    w1.dy * s,
                                                    w1.dz * s,
                                                    w1.type);
            return result;
        }

        public void Sum_SLS_char(IEnumerable<Displacement> inputs)
        {
            // Calculates Σ(φ₀Qkᵢ)
        }
        public void Sum_SLS_QP(IEnumerable<Displacement> inputs, double kdef)
        {
            // Calculates Σ(φᵢ₂Qkᵢ)∙(φᵢ₀ + kdef∙φᵢ₂)
        }
    }
}
