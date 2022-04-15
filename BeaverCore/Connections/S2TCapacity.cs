using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaverCore.Connections
{
    public enum SteelPosition
    {
        SteelIn,
        SteelOut
    }
    public class S2TCapacity : SingleFastenerCapacity
    {
        Material mat;       // steel material assigned

        public double t_steel;
        public bool thinPlate;
        public SteelPosition steelposition;

        double b;           // width in frame local axis X
        double h;           // height in frame local axis Z

        double offset_x;    // offset in frame local axis x 
        double offset_y;    // offset in frame local axis Y
        double offset_z;    // offset in frame local axis z

        double n;           // number of plates on the connection

        double alpha1;      // Angle between force acting on fastner and timber grain
        double t1;          // Timber thickness
        bool preDrilled;

        public S2TCapacity() { }

        public S2TCapacity(
            // CONSTRUCTOR FOR STEEL TO TIMBER analysis
            Fastener Fastener,
            bool PreDrilled,
            double alpha,
            double alphafast,
            Material tMat,
            double T1,
            double T_steel,
            int SD,
            SteelPosition sp
        )
        {
            this.fastener = Fastener;
            preDrilled = PreDrilled;
            this.alpha1 = alpha;
            this.mat = tMat;
            this.t1 = T1;
            this.t_steel = T_steel;
            this.sheartype = SD;
            this.alpha1 = alphafast;
            steelposition = sp;
            isThinPlate();
            GetFvk();
        }

        private void isThinPlate()
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
                capacities.Add("a", 0.4 * fhk * t1 * fastener.d);

                // Mode b
                double Fyrk2 = (1.15 * Math.Sqrt(2 * Myrk * fhk * fastener.d));
                rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyrk2) : 0;
                capacities.Add("b", Fyrk2 + rope_effect_contribution);
            }

            // THICK STEEL PLATE IN SINGLE SHEAR
            // EC5, Section 8.2.3, Eq. 8.10
            else
            {
                analysisType = "THICK STEEL PLATE IN SINGLE SHEAR - EC5, Section 8.2.3, Eq. 8.10";
                //Mode c
                capacities.Add("c", fhk * t1 * fastener.d);

                //Mode d
                double Fyrk4 = (2.3 * Math.Sqrt(Myrk * fhk * fastener.d));
                rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyrk4) : 0;
                capacities.Add("d", Fyrk4 + rope_effect_contribution);

                //Mode e
                double Fyrk5 = (fhk * t1 * fastener.d * (Math.Sqrt(2 + (4 * Myrk) / (fhk * Math.Pow(t1, 2) * fastener.d)) - 1));
                rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyrk5) : 0;
                capacities.Add("c", Fyrk5 + rope_effect_contribution);
            }

            return capacities;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public override Dictionary<string, double> FvkDoubleShear()
        {
            double maxFaxrk = this.FaxrkUpperLimitValue();

            var capacities = new Dictionary<string, double>();
            double rope_effect_contribution;

            switch (steelposition)
            {
                case SteelPosition.SteelIn:
                    // CENTRAL STEEL PLATE IN DOUBLE SHEAR
                    // EC5, Section 8.2.3, Eq. 8.11

                    analysisType = "CENTRAL STEEL PLATE IN DOUBLE SHEAR - EC5, Section 8.2.3, Eq. 8.11";

                    // Mode f
                    capacities.Add("f", fhk * t1 * fastener.d);

                    // Mode g
                    double Fyrk2 = (fhk * t1 * fastener.d * (Math.Sqrt(2 + (4 * Myrk) / (fhk * Math.Pow(t1, 2) * fastener.d)) - 1));
                    rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyrk2) : 0;
                    capacities.Add("g", Fyrk2 + rope_effect_contribution);

                    // Mode h
                    double Fyrk3 = (2.3 * Math.Sqrt(Myrk * fhk * fastener.d));
                    rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyrk3) : 0;
                    capacities.Add("h", Fyrk3 + rope_effect_contribution);

                    break;

                case SteelPosition.SteelOut:
                    if (thinPlate)
                    {
                        // OUTER THIN STEEL PLATES IN DOUBLE SHEAR 
                        // EC5, Section 8.2.3, Eq. 8.12

                        analysisType = "OUTER THIN STEEL PLATES IN DOUBLE SHEAR - EC5, Section 8.2.3, Eq. 8.12";

                        // Mode j
                        capacities.Add("j", 0.5 * fhk * fastener.d * t1);

                        // Mode k
                        double Fyrk8 = 1.15 * Math.Sqrt(2 * Myrk * fhk * fastener.d);
                        rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyrk8) : 0;
                        capacities.Add("k", Fyrk8 + rope_effect_contribution);


                    }
                    else
                    {
                        // OUTER THICK STEEL PLATES IN DOUBLE SHEAR
                        // EC5, Section 8.2.3, Eq. 8.13

                        analysisType = "OUTER THICK STEEL PLATES IN DOUBLE SHEAR - EC5, Section 8.2.3, Eq. 8.13";

                        // Mode l
                        capacities.Add("l", 0.5 * fhk * fastener.d * t1);

                        // Mode m
                        double Fyrk10 = 2.3 * Math.Sqrt(Myrk * fhk * fastener.d);
                        rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyrk10) : 0;
                        capacities.Add("m", Fyrk10 + rope_effect_contribution);

                    }
                    break;
            }

            return capacities;
        }
        public override Dictionary<string, double> Faxk()
        {
            // Calculates the axial force acting on each screw by decomposing V and N into the local axis of the fastener
            Fastener f = fastener;
            switch (f.type)
            {
                case "Nail":
                    // EC5, SECTION 8.3.2 AXIALLY LOADED NAILS
                    if (f.smooth)
                    {
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.24a", f.faxk * f.d * f.tpen);
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.24b", f.fheadk * f.d * f.t + f.fheadk * f.dh * f.dh);
                    }
                    else
                    {
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.23a", f.faxk * f.d * f.tpen);
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.23b", f.fheadk * Math.Pow(f.dh, 2));
                    }
                    break;
                case "Dowel":
                    throw new ArgumentException("Dowel does not support Axial loading");
                case "Bolted":
                    // EC5, SECTION 8.5.2 AXIALLY LOADED BOLTS
                    double faxrd = 0.9 * f.fuk * Math.Pow(f.d, 2) / 4 * 1.25 / 1.25; //***!Needs review
                    axial_capacities.Add("Faxrd, EC5, 8.5.2", faxrd);
                    break;
                case "Screw":
                    // EC5, SECTION 8.7.2 AXIALLY LOADED SCREWS
                    if (f.d > 6 & f.d < 12)
                    {
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.38", Faxrk);
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40b", 99999); //***!Missing! Implement on MultiAxialCapacity.cs
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40c", 99999); //***!Missing! Implement on MultiAxialCapacity.cs
                    }
                    else
                    {
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40a", Faxrk * Math.Pow(f.rhok / f.rhoa, 0.8));
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40b", 99999); //***!Missing! Implement on MultiAxialCapacity.cs
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40c", 99999); //***!Missing! Implement on MultiAxialCapacity.cs
                    }
                    break;
                default:
                    throw new ArgumentException("Fastener type not found");
            }
            return axial_capacities;
        }
    }
}
