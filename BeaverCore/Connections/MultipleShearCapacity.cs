using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace BeaverCore.Connections
{
    using DictResults = Dictionary<string, double>;
    public class MultipleShearFastenerCapacity
    {
        public List<SingleFastenerCapacity> fastener_capacities;
        public ShearSpacing spacing;
        public Fastener fastener;
        bool isMultiple;

        public MultipleShearFastenerCapacity(List<SingleFastenerCapacity> fastener_capacities, ShearSpacing spacing)
        {
            if (fastener_capacities.Count != spacing.npar * spacing.npep)
            {
                throw new ArgumentException("The number of fastener capacities does not match with the spacing array (npar*npep)");
            }
            if (fastener_capacities.Any(x => (x.fastener.type != fastener_capacities[0].fastener.type &&
                                                  x.fastener.d != fastener_capacities[0].fastener.d)))
            {
                throw new ArgumentException("There are different fastener types inputed. Diameter and type of fastener must be the same for all fastener capacities");
            }
            this.fastener_capacities = fastener_capacities;
            this.spacing = spacing;
            isMultiple = true;
        }

        public MultipleShearFastenerCapacity(SingleFastenerCapacity fastener_Cap, ShearSpacing spacing)
        {
            this.fastener_capacities = new List<SingleFastenerCapacity>() { fastener_Cap };
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
            SingleFastenerCapacity capacity = fastener_capacities[0];
            int npar = spacing.npar;
            int npep = spacing.npep;
            double n = npar * npep;
            double nef = Nef();
            double alpha = capacity.alfa1;
            double nalpha;
            if (capacity is T2TCapacity)
            {
                T2TCapacity t2tfast = (T2TCapacity)capacity;
                double nalpha1 = (alpha / (Math.PI / 2)) * (n - nef) + nef;
                double nalpha2 = (t2tfast.alfa2 / (Math.PI / 2)) * (n - nef) + nef;
                nalpha = Math.Min(nalpha1, nalpha2);
            }
            else
            {
                nalpha = (alpha / (Math.PI / 2)) * (n - nef) + nef;
            }
            

            
            double Util = 0;
            DictResults result = new DictResults(capacity.capacities);
            foreach (string failure in capacity.capacities.Keys.ToList())
            {
                result[failure] *= nalpha;
            }
            return result;

        }

        List<DictResults> IndividualShearResistance()
        {
            List<DictResults> result_list = new List<DictResults>();
            int npar = spacing.npar;
            int npep = spacing.npep;
            double n = npar * npep;
            double nef = Nef();
            foreach (SingleFastenerCapacity capacity in fastener_capacities)
            {
                double alpha = capacity.alfa1;
                double nalpha;
                if (capacity is T2TCapacity)
                {
                    T2TCapacity t2tfast = (T2TCapacity)capacity;
                    double nalpha1 = (alpha / (Math.PI / 2)) * (n - nef) + nef;
                    double nalpha2 = (t2tfast.alfa2 / (Math.PI / 2)) * (n - nef) + nef;
                    nalpha = Math.Min(nalpha1, nalpha2);
                }
                else
                {
                    nalpha = (alpha / (Math.PI / 2)) * (n - nef) + nef;
                }

                DictResults result = new DictResults(capacity.capacities);
                foreach (string failure in capacity.capacities.Keys.ToList())
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
            double npep = spacing.npep;

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
            if (type == "bolt" || (type == "screw" & d >= 6) || type == "dowel")
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
