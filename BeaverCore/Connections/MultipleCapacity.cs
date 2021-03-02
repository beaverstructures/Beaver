﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BeaverCore.Connections
{
    public class MultipleFastenerCapacity
    {
        SingleFastenerCapacity fastener_Cap;
        double npar;
        double npep;
        double a1;
        double a2;
        double a3c;
        double a3t;
        double a4c;
        double a4t;
        bool type; //0 for overall connection capacity, 1 for single fastener capacity

        public MultipleFastenerCapacity(SingleFastenerCapacity fastener_Cap, double npar, double npep, double a1, double a2, double a3c, double a3t, double a4c, double a4t)
        {
            this.fastener_Cap = fastener_Cap;
            this.npar = npar;
            this.npep = npep;
            this.a1 = a1;
            this.a2 = a2;
            this.a3c = a3c;
            this.a3t = a3t;
            this.a4c = a4c;
            this.a4t = a4t;
        }

        public double OverallResistance() {

            double alpha = fastener_Cap.alfa1;
            if (fastener_Cap is T2TCapacity)
            {
                T2TCapacity t2tfast = (T2TCapacity)fastener_Cap;
                alpha = Math.Min(alpha, t2tfast.alfa2);
            }
            double d = fastener_Cap.fastener.d;
            double result = 0;
            double n = npar * npep;
            double nef = Nef();
            double nalfa = (alpha / (Math.PI / 2)) * (n - nef) + nef;
            double Util = 0;
            double FVrd = nalfa * fastener_Cap.capacity.Fvk;

            return result;
        
        }

        public double IndividualResistance() {
            List<double> result = new List<double>();

            return result;
        }
        double Nef()
        {
            string type = fastener_Cap.fastener.type;
            double d = fastener_Cap.fastener.d;
            double nef = 0;
            if (type == "nail" || (type == "screw" & d < 6))
            {

                double kef = 0;
                if (a1 >= 4 * d & a1 < 7 * d)
                {
                    kef = 0.5 - (0.5 - 0.7) * (4 * d - a1) / (4 * d - 7 * d);
                }
                if (a1 >= 7 * d & a1 < 10 * d)
                {
                    kef = 0.7 - (0.7 - 0.85) * (7 * d - a1) / (7 * d - 10 * d);
                }
                if (a1 >= 10 * d & a1 < 14 * d)
                {
                    kef = 0.85 - (0.85 - 1) * (10 * d - a1) / (10 * d - 14 * d);
                }
                if (a1 >= 14 * d)
                {
                    kef = 1;
                }
                nef = (Math.Pow(npar, kef)) * npep;
            }
            if (type == "bolt" || (type == "screw" & d >= 6) || type =="dowel")
            {
                if (npar * npep == 1) { nef = 1; }
                else
                {
                    nef = Math.Min(npar * npep, Math.Pow(npar, 0.9) * Math.Pow(a1 / (13 * d), 0.25)) * npep;
                }
            }
            return nef;
        }
    }
}
