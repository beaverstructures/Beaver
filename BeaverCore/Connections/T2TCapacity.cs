using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeaverCore.Connections
{
    public class T2TCapacity: SingleFastenerCapacity
    {
        // Calculates the capacity of Timber-to-timber connections according to EC5, Section 8.2.2
        public double alfa2;
        public double t2;
        public double pk2;
        public double t_head;
        public double t_thread;
        public string woodType;

        public T2TCapacity() { }

        //to shear connections
        public T2TCapacity(
            Fastener Fastener,
            double T1,
            double T2,
            double Alfa1,
            double Alfa2,
            string TimberMaterial,
            string ConnectorMaterial,
            bool PreDrilled,
            double Pk1,
            double Pk2,
            double T_head,
            string WoodType,
            double T_thread,
            double Alfafast
            
        )
        {
            this.t1 = T1;
            this.t2 = T2;
            this.alfa1 = Alfa1;
            this.alfa2 = Alfa2;
            this.fastener = Fastener;
            this.timberMaterial = TimberMaterial;
            this.connectorMaterial = ConnectorMaterial;
            this.preDrilled = PreDrilled;
            this.pk1 = Pk1;
            this.pk2 = Pk2;
            this.woodType = WoodType;
            this.t_head = T_head;
            this.t_thread = T_thread;
            this.alfafast = Alfafast;
            this.variables = new Variables(Fastener, PreDrilled, Pk1, Pk2, Alfa1, Alfa2, alfafast, WoodType, T1, T2, T_thread);
            GetFvk();
  
        }

        public override Dictionary<string, double> FvkSingleShear()
        {
            /// Calculates fastener capacity for single shear according to EC5, Section 8.2.2, Eq. 8.6

            analysisType = "FASTENERS IN SINGLE SHEAR - EC5, Section 8.2.2, Eq. 8.6";

            double maxFaxrk = FaxrkUpperLimitValue();
            double Mryk = this.variables.Myrk;
            double Fh1k = this.variables.fh1k;
            double Fh2k = this.variables.fh2k;
            double Beta = this.variables.beta;
            double Faxrk = this.variables.Faxrk;

            var capacities = new Dictionary<string, double>();
            double rope_effect_contribution;

            //1st Failure Mode (a)
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6a", Fh1k * t1 * this.fastener.d);

            //2nd Failure Mode (b)
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6b", Fh2k * t2 * this.fastener.d);

            //3rd Failure Mode (c)
            double Fyrk3 = ((Fh1k * t1 * this.fastener.d) / (1 + Beta))
                * (Math.Sqrt(Beta + 2 * Math.Pow(Beta, 2) * (1 + (t2 / t1) + Math.Pow(t2 / t1, 2)) + Math.Pow(Beta, 3) * Math.Pow(t2 / t1, 2)) - Beta * (1 + (t2 / t1)));
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk*Fyrk3) : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6c", Fyrk3 + rope_effect_contribution);

            //4th Failure Mode (d)
            double Fyk4 = ((1.05 * Fh1k * t1 * this.fastener.d) / (2 + Beta))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + ((4 * Beta * (2 + Beta) * Mryk) / (Fh1k * Math.Pow(t1, 2) * this.fastener.d))) - Beta);
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk*Fyk4) : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6d", Fyk4 + rope_effect_contribution);

            //5th Failure Mode (e)
            double Fyk5 = ((1.05 * Fh2k * t2 * this.fastener.d) / (2 + Beta))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + ((4 * Beta * (2 + Beta) * Mryk) / (Fh2k * Math.Pow(t2, 2) * this.fastener.d))) - Beta);
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyk5) : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6e", Fyk5 + rope_effect_contribution);

            //6th Failure Mode (f)
            double Fyk6 = 1.15 * Math.Sqrt((2 * Beta) / (1 + Beta))
                * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d);
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyk6) : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.6f", Fyk6 + rope_effect_contribution);

            return capacities;
        }

        public override Dictionary<string, double> FvkDoubleShear()
        {
            /// Calculates fastener capacity for double shear according to EC5, Section 8.2.2, Eq. 8.7
            analysisType = "FASTENERS IN DOUBLE SHEAR - EC5, Section 8.2.2, Eq. 8.7";

            double maxFaxrk = FaxrkUpperLimitValue();
            double Mryk = this.variables.Myrk;
            double Fh1k = this.variables.fh1k;
            double Fh2k = this.variables.fh2k;
            double Beta = this.variables.beta;
            double Faxrk = this.variables.Faxrk;
            
            var capacities = new Dictionary<string, double>();
            double rope_effect_contribution;

            // 1st Failure Mode (g)
            capacities.Add("EC5, Section 8.2.2, Eq. 8.7g", Fh1k * t1 * this.fastener.d);

            // 2nd Failure Mode (h)
            capacities.Add("EC5, Section 8.2.2, Eq. 8.7h", 0.5 * Fh2k * t2 * this.fastener.d);

            // 3rd Failure Mode (j)
            double Fyk3 = (1.05 * ((Fh1k * t1 * this.fastener.d) / (2 * Beta)))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + (4 * Beta * (2 + Beta) * Mryk) / (Fh1k * Math.Pow(t1, 2) * this.fastener.d)) - Beta);
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyk3) : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.7j", Fyk3 + rope_effect_contribution);

            // 4th Failure Mode (k)
            double Fyk4 = (1.15 * Math.Sqrt((2 * Beta) / (1 + Beta)) * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d));
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyk4) : 0;
            capacities.Add("EC5, Section 8.2.2, Eq. 8.7k", Fyk4 + rope_effect_contribution);

            return capacities;
        }

        public override Dictionary<string, double> Faxk()
        {
            // Calculates the axial force acting on each screw by decomposing V and N into the local axis of the fastener
            switch (this.fastener.type)
            {
                case "Screw":
                    // EC5, SECTION 8.7.2 AXIALLY LOADED SCREWS
                    capacities.Add("Faxrd, EC5 Eq. 8.38", this.variables.Faxrk);
                    capacities.Add("Faxrd, EC5 Eq. 8.40b", 0); //***!Missing
                    capacities.Add("Faxrd, EC5 Eq. 8.40c", 0); //***!Missing
                    break;
                case "Nail":
                    if (this.fastener.smooth)
                    {
                        capacities.Add("Faxrd, EC5 Eq. 8.23a", 0); //***!Missing
                        capacities.Add("Faxrd, EC5 Eq. 8.23b", 0); //***!Missing
                    }
                    else
                    {
                        capacities.Add("Faxrd, EC5 Eq. 8.24a", 0); //***!Missing
                        capacities.Add("Faxrd, EC5 Eq. 8.24b", 0); //***!Missing
                    }
                    break;
                case "Dowel":
                    double faxrd = 0.9 * fastener.fu * Math.Pow(fastener.d,2)/4;
                    capacities.Add("Faxrd", 0.9*fastener.fu); //***!Missing
                    break;
                case "Bolted":
                    capacities.Add("Faxrd", 0); //***!Missing
                    break;
                default:
                    throw new ArgumentException("Timber material type not found");
            } 
            capacities.Add("Fax",variables.Faxrk);
            return capacities;
        }
    


    }
}
