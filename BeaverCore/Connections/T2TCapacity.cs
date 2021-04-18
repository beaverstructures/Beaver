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
  
        }

        public override void GetFvk()
        {
            if (base.sheartype == 1)   capacities = FvkSingleShear();
            else if (base.sheartype == 2) capacities = FvkDoubleShear();
            else { throw new ArgumentException(); }
        }

        public override Dictionary<string, double> FvkSingleShear()
        {
            /// Calculates fastener capacity for single shear according to EC5, Section 8.2.2, Eq. 8.6
            double maxFaxrk = FaxrkUpperLimitValue();
            double Mryk = this.variables.Myrk;
            double Fh1k = this.variables.fh1k;
            double Fh2k = this.variables.fh2k;
            double Beta = this.variables.beta;
            double Faxrk = this.variables.Faxrk;

            var capacities = new Dictionary<string, double>();
            double rope_effect_contribution;

            //1º modo (a)
            capacities.Add("a", Fh1k * t1 * this.fastener.d);

            //2º modo (b)
            capacities.Add("b", Fh2k * t2 * this.fastener.d);

            //3º modo (c)
            double Fyrk3 = ((Fh1k * t1 * this.fastener.d) / (1 + Beta))
                * (Math.Sqrt(Beta + 2 * Math.Pow(Beta, 2) * (1 + (t2 / t1) + Math.Pow(t2 / t1, 2)) + Math.Pow(Beta, 3) * Math.Pow(t2 / t1, 2)) - Beta * (1 + (t2 / t1)));
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk*Fyrk3) : 0;
            capacities.Add("c", Fyrk3 + rope_effect_contribution);

            //4º modo (d)
            double Fyk4 = ((1.05 * Fh1k * t1 * this.fastener.d) / (2 + Beta))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + ((4 * Beta * (2 + Beta) * Mryk) / (Fh1k * Math.Pow(t1, 2) * this.fastener.d))) - Beta);
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk*Fyk4) : 0;
            capacities.Add("d", Fyk4 + rope_effect_contribution);

            //5º modo (e)
            double Fyk5 = ((1.05 * Fh2k * t2 * this.fastener.d) / (2 + Beta))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + ((4 * Beta * (2 + Beta) * Mryk) / (Fh2k * Math.Pow(t2, 2) * this.fastener.d))) - Beta);
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyk5) : 0;
            capacities.Add("e", Fyk5 + rope_effect_contribution);

            //6º modo (f)
            double Fyk6 = 1.15 * Math.Sqrt((2 * Beta) / (1 + Beta))
                * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d);
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyk6) : 0;
            capacities.Add("f", Fyk6 + rope_effect_contribution);

            return capacities;
        }

        public override Dictionary<string, double> FvkDoubleShear()
        {
            double maxFaxrk = FaxrkUpperLimitValue();
            double Mryk = this.variables.Myrk;
            double Fh1k = this.variables.fh1k;
            double Fh2k = this.variables.fh2k;
            double Beta = this.variables.beta;
            double Faxrk = this.variables.Faxrk;
            
            var capacities = new Dictionary<string, double>();
            double rope_effect_contribution;

            // 1º mode (g)
            capacities.Add("g", Fh1k * t1 * this.fastener.d);

            // 2º mode (h)
            capacities.Add("h", 0.5 * Fh2k * t2 * this.fastener.d);

            // 3º mode (j)
            double Fyk3 = (1.05 * ((Fh1k * t1 * this.fastener.d) / (2 * Beta)))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + (4 * Beta * (2 + Beta) * Mryk) / (Fh1k * Math.Pow(t1, 2) * this.fastener.d)) - Beta);
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyk3) : 0;
            capacities.Add("j", Fyk3 + rope_effect_contribution);

            // 4º mode (k)
            double Fyk4 = (1.15 * Math.Sqrt((2 * Beta) / (1 + Beta)) * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d));
            rope_effect_contribution = rope_effect ? Math.Min(Faxrk / 4, maxFaxrk * Fyk4) : 0;
            capacities.Add("k", Fyk4 + rope_effect_contribution);

            return capacities;
        }

    


    }
}
