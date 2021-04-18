using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaverCore.Connections
{
    public class T2SCapacity : SingleFastenerCapacity
    {
        public double t_steel;
        public bool thinPlate;

        public T2SCapacity() { }

        public T2SCapacity(
            Fastener Fastener,
            bool PreDrilled,
            double Pk,
            double Alfa,
            double Alfafast,
            Material tMat,
            double T1,
            double T_steel,
            double T_thread,
            int SD
        )
        {
            this.fastener = Fastener;
            preDrilled = PreDrilled;
            pk1 = Pk;
            this.alfa1 = Alfa;
            this.tMat1 = tMat;
            this.t1 = T1;
            this.t_steel = T_steel;
            this.sheartype = SD;
            this.variables = new Variables(Fastener, PreDrilled, Pk, Alfa, Alfafast, tMat1.type, T1, T_steel, T_thread);
            this.alfafast = Alfafast;
            isThinPlate();
        }

        public void isThinPlate()
        {
            // check if the plate is classified as thick or thin
            thinPlate = (t_steel <= 0.5 * fastener.d);
        }

        public override Dictionary<string, double> FvkSingleShear()
        {
            double maxFaxrk = this.FaxrkUpperLimitValue();

            var capacities = new Dictionary<string, double>();
            double rope_effect_contribution;

            // THIN STEEL PLATE IN SINGLE SHEAR
            // EC5, Section 8.2.3, Eq. 8.9

            if (thinPlate)
            {
                analysisType = "THIN STEEL PLATE IN SINGLE SHEAR - EC5, Section 8.2.3, Eq. 8.9";

                // Mode a
                capacities.Add("a", 0.4 * variables.fhk * t1 * fastener.d);

                // Mode b
                double Fyrk2 = (1.15 * Math.Sqrt(2 * variables.Myrk * variables.fhk * fastener.d));
                rope_effect_contribution = rope_effect ? Math.Min(variables.Faxrk / 4, maxFaxrk * Fyrk2) : 0;
                capacities.Add("b", Fyrk2 + rope_effect_contribution);
            }

            // THICK STEEL PLATE IN SINGLE SHEAR
            // EC5, Section 8.2.3, Eq. 8.10
            else
            {
                analysisType = "THICK STEEL PLATE IN SINGLE SHEAR - EC5, Section 8.2.3, Eq. 8.10";
                //Mode c
                capacities.Add("c", variables.fhk * t1 * fastener.d);

                //Mode d
                double Fyrk4 = (2.3 * Math.Sqrt(variables.Myrk * variables.fhk * fastener.d));
                rope_effect_contribution = rope_effect ? Math.Min(variables.Faxrk / 4, maxFaxrk * Fyrk4) : 0;
                capacities.Add("d", Fyrk4 + rope_effect_contribution);

                //Mode e
                double Fyrk5 = (variables.fhk * t1 * fastener.d * (Math.Sqrt(2 + (4 * variables.Myrk) / (variables.fhk * Math.Pow(t1, 2) * fastener.d)) - 1));
                rope_effect_contribution = rope_effect ? Math.Min(variables.Faxrk / 4, maxFaxrk * Fyrk5) : 0;
                capacities.Add("c", Fyrk5 + rope_effect_contribution);
            }

            return capacities;
        }

        public override Dictionary<string, double> FvkDoubleShear()
        {
            double maxFaxrk = this.FaxrkUpperLimitValue();

            var capacities = new Dictionary<string, double>();
            double rope_effect_contribution;

            switch (sheartype)
            {
                case 1:
                    // CENTRAL STEEL PLATE IN DOUBLE SHEAR
                    // EC5, Section 8.2.3, Eq. 8.11

                    analysisType = "CENTRAL STEEL PLATE IN DOUBLE SHEAR - EC5, Section 8.2.3, Eq. 8.11";

                    // Mode f
                    capacities.Add("f", variables.fhk * t1 * fastener.d);

                    // Mode g
                    double Fyrk2 = (variables.fhk * t1 * fastener.d * (Math.Sqrt(2 + (4 * variables.Myrk) / (variables.fhk * Math.Pow(t1, 2) * fastener.d)) - 1));
                    rope_effect_contribution = rope_effect ? Math.Min(variables.Faxrk / 4, maxFaxrk * Fyrk2) : 0;
                    capacities.Add("g", Fyrk2 + rope_effect_contribution);

                    // Mode h
                    double Fyrk3 = (2.3 * Math.Sqrt(variables.Myrk * variables.fhk * fastener.d));
                    rope_effect_contribution = rope_effect ? Math.Min(variables.Faxrk / 4, maxFaxrk * Fyrk3) : 0;
                    capacities.Add("h", Fyrk3 + rope_effect_contribution);

                    break;

                case 2:
                    if (thinPlate)
                    {
                        // OUTER THIN STEEL PLATES IN DOUBLE SHEAR 
                        // EC5, Section 8.2.3, Eq. 8.12

                        analysisType = "OUTER THIN STEEL PLATES IN DOUBLE SHEAR - EC5, Section 8.2.3, Eq. 8.12";

                        // Mode j
                        capacities.Add("j", 0.5 * variables.fhk * fastener.d * t1);

                        // Mode k
                        double Fyrk8 = 1.15 * Math.Sqrt(2 * variables.Myrk * variables.fhk * fastener.d);
                        rope_effect_contribution = rope_effect ? Math.Min(variables.Faxrk / 4, maxFaxrk * Fyrk8) : 0;
                        capacities.Add("k", Fyrk8 + rope_effect_contribution);


                    }
                    else
                    {
                        // OUTER THICK STEEL PLATES IN DOUBLE SHEAR
                        // EC5, Section 8.2.3, Eq. 8.13

                        analysisType = "OUTER THICK STEEL PLATES IN DOUBLE SHEAR - EC5, Section 8.2.3, Eq. 8.13";

                        // Mode l
                        capacities.Add("l", 0.5 * variables.fhk * fastener.d * t1);

                        // Mode m
                        double Fyrk10 = 2.3 * Math.Sqrt(variables.Myrk * variables.fhk * fastener.d);
                        rope_effect_contribution = rope_effect ? Math.Min(variables.Faxrk / 4, maxFaxrk * Fyrk10) : 0;
                        capacities.Add("m", Fyrk10 + rope_effect_contribution);

                    }
                    break;
            }

            return capacities;
        }
    }
}
