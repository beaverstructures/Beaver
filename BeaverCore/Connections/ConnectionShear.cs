
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using BeaverCore.Misc;
using BeaverCore.Geometry;
using BeaverCore.Actions;


namespace BeaverCore.Connections
{
    using DictResults = Dictionary<string, double>;
    public enum ConnectionType
    {
        TimbertoTimber,
        TimbertoSteel
    }

    [Serializable]
    public class ConnectionShearFastenerCapacity : Connection
    {
        public SingleFastenerCapacity fastener_capacity;
        public ShearSpacing spacing;
        public Fastener fastener;
        bool isMultiple;

        public ConnectionShearFastenerCapacity(SingleFastenerCapacity fastener_Cap, ShearSpacing spacing)
        {
            this.fastener_capacity = fastener_Cap ;
            this.spacing = spacing;
            this.fastener = fastener_Cap.fastener;
            isMultiple = false;
        }

        public List<DictResults> ShearResistance()
        {
            if (isMultiple)
            {
                return new List<DictResults>() { OverallShearResistance() };
            }
            else
            {
                return IndividualShearResistance();
            }
        }

        DictResults OverallShearResistance()
        {
            SingleFastenerCapacity capacity = fastener_capacity;
            int npar = spacing.npar;
            int nperp = spacing.nperp;
            double n = npar * nperp;
            double nef = Nef();
            double alpha = capacity.fastener.alpha;
            double nalpha;
            if (capacity is T2TCapacity)
            {
                T2TCapacity t2tfast = (T2TCapacity)capacity;
                double nalpha1 = (alpha / (Math.PI / 2)) * (n - nef) + nef;
                double nalpha2 = (t2tfast.alpha2 / (Math.PI / 2)) * (n - nef) + nef;
                nalpha = Math.Min(nalpha1, nalpha2);
            }
            else
            {
                nalpha = (alpha / (Math.PI / 2)) * (n - nef) + nef;
            }
            

            
            DictResults result = new DictResults(capacity.shear_capacities);
            foreach (string failure in capacity.shear_capacities.Keys.ToList())
            {
                result[failure] *= nalpha;
            }
            return result;

        }

        List<DictResults> IndividualShearResistance()
        {
            List<DictResults> result_list = new List<DictResults>();
            int npar = spacing.npar;
            int nperp = spacing.nperp;
            double n = npar * nperp;
            double nef = Nef();
            List<SingleFastenerCapacity> fastener_capacities = new List<SingleFastenerCapacity>() { fastener_capacity };
            foreach (SingleFastenerCapacity capacity in fastener_capacities)
            {
                double alpha = capacity.fastener.alpha;
                double nalpha;
                if (capacity is T2TCapacity)
                {
                    T2TCapacity t2tfast = (T2TCapacity)capacity;
                    double nalpha1 = (alpha / (Math.PI / 2)) * (n - nef) + nef;
                    double nalpha2 = (t2tfast.alpha2 / (Math.PI / 2)) * (n - nef) + nef;
                    nalpha = Math.Min(nalpha1, nalpha2);
                }
                else
                {
                    nalpha = (alpha / (Math.PI / 2)) * (n - nef) + nef;
                }

                DictResults result = new DictResults(capacity.shear_capacities);
                foreach (string failure in capacity.shear_capacities.Keys.ToList())
                {
                    result[failure] *= nalpha/n;
                }
                result_list.Add(result);
            }
            return result_list;
        }

        double Nef()
        {
            string type = fastener.type;
            double d = fastener.d;
            double nef = 0;
            double a1 = spacing.a1;
            double npar = spacing.npar;
            double nperp = spacing.nperp;

            if (type == "nail" || (type == "screw" & d < 6))
            {

                double kef = 0;
                switch (a1)
                {
                    case double n when n < 4*d: 
                        break;
                    case double n when n >= 4 * d & n < 7 * d:
                        kef = 0.5 - (0.5 - 0.7) * (4 * d - a1) / (4 * d - 7 * d); break;
                    case double n when n >= 7 * d & n < 10 * d:
                        kef = 0.7 - (0.7 - 0.85) * (7 * d - a1) / (7 * d - 10 * d); break;
                    case double n when n >= 10 * d & n< 14 * d:
                        kef = 0.85 - (0.85 - 1) * (10 * d - a1) / (10 * d - 14 * d); break;
                    case double n when n >= 14 * d:
                        kef = 1; break;

                }
                nef = (Math.Pow(npar, kef)) * nperp;
            }
            else if (type == "bolt" || (type == "screw" & d >= 6) || type == "dowel")
            {
                if (npar * nperp == 1) { nef = 1; }
                else
                {
                    nef = Math.Min(npar * nperp, Math.Pow(npar, 0.9) * Math.Pow(a1 / (13 * d), 0.25)) * nperp;
                }
            }
            return nef;
        }
    }
}
