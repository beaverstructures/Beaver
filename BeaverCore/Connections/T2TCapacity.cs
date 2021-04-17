using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeaverCore.Connections
{
    public class T2TCapacity: SingleFastenerCapacity
    {
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
            double Alfafast,
            double Npar,
            double Npep,
            double A1,
            bool type
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

        public override void GetFvk(bool type)
        {
            if (sheartype == 0) capacity = FvkSingleShear(type);
            else capacity = FvkDoubleShear(type);
        }

        public override object FvkSingleShear(bool type)
        {
            double maxFaxrk = FaxrkUpperLimitValue();
            double Mryk = this.variables.Myrk;
            double Fh1k = this.variables.fh1k;
            double Fh2k = this.variables.fh2k;
            double Beta = this.variables.beta;
            double Faxrk = this.variables.Faxrk;
            double Fvk;
            string failureMode = "";
            List<double> Fvrks = new List<double>();
            List<string> failures = new List<string>();
            //1º modo (a)
            double Fvk1 =  Fh1k * t1 * this.fastener.d;
            Fvk = Fvk1;
            failureMode = "a";
            Fvrks.Add(Fvk1);
            failures.Add("a");
            //2º modo (b)
            double Fvk2 =  Fh2k * t2 * this.fastener.d;
            if (Fvk > Fvk2)
            {
                Fvk = Fvk2;
                failureMode = "b";
            }
            Fvrks.Add(Fvk2);
            failures.Add("b");
            //3º modo (c)
            double Fvrk3 = 0;
            double Fyrk3 = ((Fh1k * t1 * this.fastener.d) / (1 + Beta))
                * (Math.Sqrt(Beta + 2 * Math.Pow(Beta, 2) * (1 + (t2 / t1) + Math.Pow(t2 / t1, 2)) + Math.Pow(Beta, 3) * Math.Pow(t2 / t1, 2)) - Beta * (1 + (t2 / t1)));
            Fvrk3 = Math.Min(Fyrk3 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyrk3);
            Fvrks.Add(Fvrk3);
            failures.Add("c");
            if (Fvk > Fvrk3)
            {
                Fvk = Fvrk3;
                failureMode = "c";
            }

            //4º modo (d)
            double Fvk4 = 0;
            double Fyk4 = ((1.05 * Fh1k * t1 * this.fastener.d) / (2 + Beta))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + ((4 * Beta * (2 + Beta) * Mryk) / (Fh1k * Math.Pow(t1, 2) * this.fastener.d))) - Beta);
            Fvk4 = Math.Min(Fyk4 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyk4);
            Fvrks.Add(Fvk4);
            failures.Add("d");
            if (Fvk > Fvk4)
            {
                Fvk = Fvk4;
                failureMode = "d";
            }

            //5º modo (e)
            double Fvk5 = 0;
            double Fyk5 = ((1.05 * Fh2k * t2 * this.fastener.d) / (2 + Beta))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + ((4 * Beta * (2 + Beta) * Mryk) / (Fh2k * Math.Pow(t2, 2) * this.fastener.d))) - Beta);
            Fvk5 = Math.Min(Fyk5 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyk5);
            Fvrks.Add(Fvk5);
            failures.Add("e");
            if (Fvk > Fvk5)
            {
                Fvk = Fvk5;
                failureMode = "e";
            }

            //6º modo (f)
            double Fvk6 = 0;
            double Fyk6 = 1.15 * Math.Sqrt((2 * Beta) / (1 + Beta))
                * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d);
            Fvk6 = Math.Min(Fyk6 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyk6);
            Fvrks.Add(Fvk6);
            failures.Add("f");
            if (Fvk > Fvk6)
            {
                Fvk = Fvk6;
                failureMode = "f";
            }

            if (type)
            {
                return new
                {
                    Fvk,
                    failureMode
                };
            }
            else
            {
                return new
                {
                    Fvrks,
                    failures
                };
            }
        }

        public override object FvkDoubleShear(bool type)
        {
            double maxFaxrk = FaxrkUpperLimitValue();
            double Mryk = this.variables.Myrk;
            double Fh1k = this.variables.fh1k;
            double Fh2k = this.variables.fh2k;
            double Beta = this.variables.beta;
            double Faxrk = this.variables.Faxrk;
            double Fvk;
            string failureMode = "";
            List<double> Fvrks = new List<double>();
            List<string> failures = new List<string>();
            // 1º mode (g)
            double Fvk1 =  (Fh1k * t1 * this.fastener.d);
            Fvk = Fvk1;
            failureMode = "g";
            Fvrks.Add(Fvk1);
            failures.Add("g");
            // 2º mode (h)
            double Fvk2 =  (0.5 * Fh2k * t2 * this.fastener.d);
            Fvrks.Add(Fvk2);
            failures.Add("h");
            if (Fvk > Fvk2)
            {
                Fvk = Fvk2;
                failureMode = "h";
            }

            // 3º mode (j)
            double Fvk3 = 0;
            double Fyk3 = (1.05 * ((Fh1k * t1 * this.fastener.d) / (2 * Beta)))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + (4 * Beta * (2 + Beta) * Mryk) / (Fh1k * Math.Pow(t1, 2) * this.fastener.d)) - Beta);
            Fvk3 = Math.Min(Fyk3 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyk3);
            Fvrks.Add(Fvk3);
            failures.Add("j");
            if (Fvk > Fvk3)
            {
                Fvk = Fvk3;
                failureMode = "j";
            }

            // 4º mode (k)
            double Fvk4 = 0;
            double Fyk4 = (1.15 * Math.Sqrt((2 * Beta) / (1 + Beta)) * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d));
            Fvk4 = Math.Min(Fyk4 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyk4);
            Fvrks.Add(Fvk4);
            failures.Add("k");
            if (Fvk > Fvk4)
            {
                Fvk = Fvk4;
                failureMode = "k";
            }

            if (type)
            {
                return new
                {
                    Fvk,
                    failureMode
                };
            }
            else
            {
                return new
                {
                    Fvrks,
                    failures
                };
            }
        }

    


    }
}
