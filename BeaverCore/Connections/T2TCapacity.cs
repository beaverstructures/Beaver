using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeaverCore.Materials;

namespace BeaverCore.Connections
{
    [Serializable]
    public class T2TCapacity: SingleFastenerCapacity
    {
        // Calculates the capacity of Timber-to-timber connections according to EC5, Section 8.2.2
        public double t1;
        public double t2;
        public Material mat1;
        public Material mat2;
        public double alpha1;
        public double alpha2;
        public string woodType;
        public bool predrillingNeeded;

        public T2TCapacity() { }

        //to shear connections
        public T2TCapacity(
            // CONSTRUCTOR FOR TIMBER TO TIMBER analysis
            Fastener fastener,
            bool preDrilled1,
            bool preDrilled2,
            Material mat1,
            Material mat2,
            double alpha1,   //
            double alpha2,
            double alphafast,
            double t1,
            double t2,
            int shearplanes,
            bool rope_effect
            )
        {
            this.fastener = fastener;
            this.shearplanes = shearplanes;
            this.rope_effect = rope_effect;

            Myrk = CalcMyrk(fastener);
            fh1k = CalcFhk(preDrilled1, fastener, mat1.pk, alpha1, mat1.type);
            fh2k = CalcFhk(preDrilled2, fastener, mat2.pk, alpha2, mat2.type);
            beta = fh2k / fh1k;
            tpen = GetTpen(fastener, t1, t2);
            Faxrk = CalcFaxrk(mat1.pk, fastener, t1, this.tpen, alphafast, fastener.lth);
            kser = CalcKser(fastener, mat1, mat2);
            kdef = CalcKdef(mat1, mat2);

            if (shearplanes == 1) { shear_capacities = FvkSingleShear(); }
            else if (shearplanes == 2) { shear_capacities = FvkDoubleShear(); }
            else { throw new ArgumentException("Number of shear planes is invalid"); }

            axial_capacities = Faxk();

            SetCriticalCapacity();
            predrillingNeeded = checkPreDrilling();
            if (predrillingNeeded || preDrilled1) { }
            if (predrillingNeeded || preDrilled2) { }

        }

        public override Dictionary<string, double> FvkSingleShear()
        {
            /// Calculates fastener capacity for single shear according to EC5, Section 8.2.2, Eq. 8.6

            analysisType = "FASTENERS IN SINGLE SHEAR - EC5, Section 8.2.2, Eq. 8.6";

            double maxFaxrk = FaxrkUpperLimitValue();
            double Mryk = this.Myrk;
            double Fh1k = this.fh1k;
            double Fh2k = this.fh2k;
            double Beta = this.beta;
            double Faxrk = this.Faxrk;

            var capacities = new Dictionary<string, double>();
            double rope_effect_contribution;

            //1st Failure Mode (a)
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6a", Fh1k * t1 * this.fastener.d);

            //2nd Failure Mode (b)
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6b", Fh2k * t2 * this.fastener.d);

            //3rd Failure Mode (c)
            double Fyrk3 = Fh1k * t1 * this.fastener.d / (1 + Beta)
                * (Math.Sqrt(Beta + 2 * Math.Pow(Beta, 2) * (1 + (t2 / t1) + Math.Pow(t2 / t1, 2)) 
                + Math.Pow(Beta, 3) * Math.Pow(t2 / t1, 2)) 
                - Beta * (1 + (t2 / t1)));
            rope_effect_contribution = rope_effect ? 
                                       Math.Min(Faxrk / 4, maxFaxrk*Fyrk3) 
                                       : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6c", Fyrk3 + rope_effect_contribution);

            //4th Failure Mode (d)
            double Fyk4 = 1.05 * Fh1k * t1 * this.fastener.d / (2 + Beta)
                * (Math.Sqrt(2 * Beta * (1 + Beta) + (4 * Beta * (2 + Beta) * Mryk / (Fh1k * Math.Pow(t1, 2) * this.fastener.d))) - Beta);
            rope_effect_contribution =  rope_effect ?
                                        Math.Min(Faxrk / 4, maxFaxrk*Fyk4) 
                                        : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6d", Fyk4 + rope_effect_contribution);

            //5th Failure Mode (e)
            double Fyk5 = 1.05 * Fh2k * t2 * this.fastener.d / (2 + Beta)
                * (Math.Sqrt(2 * Beta * (1 + Beta) + (4 * Beta * (2 + Beta) * Mryk / (Fh2k * Math.Pow(t2, 2) * this.fastener.d))) - Beta);
            rope_effect_contribution = rope_effect ?
                    Math.Min(Faxrk / 4, maxFaxrk * Fyk5) 
                    : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6e", Fyk5 + rope_effect_contribution);

            //6th Failure Mode (f)
            double Fyk6 = 1.15 * Math.Sqrt(2 * Beta / (1 + Beta))
                * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d);
            rope_effect_contribution = rope_effect ? 
                Math.Min(Faxrk / 4, maxFaxrk * Fyk6) 
                : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6f", Fyk6 + rope_effect_contribution);

            return capacities;
        }

        public override Dictionary<string, double> FvkDoubleShear()
        {
            /// Calculates fastener capacity for double shear according to EC5, Section 8.2.2, Eq. 8.7
            analysisType = "FASTENERS IN DOUBLE SHEAR - EC5, Section 8.2.2, Eq. 8.7";

            double maxFaxrk = FaxrkUpperLimitValue();
            double Mryk = this.Myrk;
            double Fh1k = this.fh1k;
            double Fh2k = this.fh2k;
            double Beta = this.beta;
            double Faxrk = this.Faxrk;
            
            var capacities = new Dictionary<string, double>();
            double rope_effect_contribution;

            // 1st Failure Mode (g)
            capacities.Add("EC5, Section 8.2.2, Eq. 8.7g", Fh1k * t1 * this.fastener.d);

            // 2nd Failure Mode (h)
            capacities.Add("EC5, Section 8.2.2, Eq. 8.7h", 0.5 * Fh2k * t2 * this.fastener.d);

            // 3rd Failure Mode (j)
            double Fyk3 = 1.05 * (Fh1k * t1 * this.fastener.d / (2 * Beta))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + 4 * Beta * (2 + Beta) * Mryk / (Fh1k * Math.Pow(t1, 2) * this.fastener.d)) - Beta);
            rope_effect_contribution = rope_effect ?
                Math.Min(Faxrk / 4, maxFaxrk * Fyk3) 
                : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.7j", Fyk3 + rope_effect_contribution);

            // 4th Failure Mode (k)
            double Fyk4 = 1.15 * Math.Sqrt(2 * Beta / (1 + Beta)) * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d);
            rope_effect_contribution = rope_effect ? 
                Math.Min(Faxrk / 4, maxFaxrk * Fyk4) 
                : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.7k", Fyk4 + rope_effect_contribution);

            return capacities;
        }

        public override Dictionary<string, double> Faxk()
        {
            // Calculates the axial force acting on each screw by decomposing V and N into the local axis of the fastener
            Fastener f = fastener;
            Dictionary<string, double> axial_capacities = new Dictionary<string, double>();
            switch (f.type)
            {
                case "Nail":
                    // EC5, SECTION 8.3.2 AXIALLY LOADED NAILS
                    if (f.smooth)
                    {
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.24a", f.faxk*f.d*f.tpen);
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.24b", f.fheadk*f.d*f.t + f.fheadk*f.dh*f.dh);
                    }
                    else
                    {
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.23a", f.faxk * f.d * f.tpen);
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.23b", f.fheadk * Math.Pow(f.dh, 2));
                    }
                    break;
                case "Dowel":
                        axial_capacities.Add("Dowel does not support axial loading", 0);
                        break;
                case "Bolted":
                    // EC5, SECTION 8.5.2 AXIALLY LOADED BOLTS
                    double faxrd = 0.9 * f.fu * Math.Pow(f.d, 2) / 4 * 1.25 / 1.25; //***!Needs review
                    axial_capacities.Add("Faxrd, EC5, 8.5.2", faxrd);
                    break;
                case "Screw":
                    // EC5, SECTION 8.7.2 AXIALLY LOADED SCREWS
                    if(f.d>6 & f.d < 12)
                    {
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.38", Faxrk);
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40b", 99999); //***!Missing! Implement on MultiAxialCapacity.cs
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40c", 99999); //***!Missing! Implement on MultiAxialCapacity.cs
                    }
                    else
                    {
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40a", Faxrk*Math.Pow(f.rhok/f.rhoa,0.8));
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40b", 99999); //***!Missing! Implement on MultiAxialCapacity.cs
                        axial_capacities.Add("Faxrd, EC5 Eq. 8.40c", 99999); //***!Missing! Implement on MultiAxialCapacity.cs
                    }
                    break;
                default:
                    throw new ArgumentException("Fastener type not found");
            } 
            return axial_capacities;
        }
    
        public bool checkPreDrilling()
        {
            bool result = false;
            double pk;
            switch (fastener.type)
            {
                case "Nail":
                case "Staple":
                case "Screws":
                    pk = Math.Min(mat1.pk, mat2.pk);
                    if (pk > 500 || fastener.d > 6) { result = true; }
                    break;
                default:
                    break;
            }
            return result;
        }

    }
}
