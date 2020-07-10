using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beaver_v0._1
{
    class Ccalc_T2TCapacity
    {
        public Ccalc_Fastener fastener;
        public double t1;
        public double t2;
        public double alfa1;
        public double alfa2;
        public double alfafast;
        public string timberMaterial;
        public string connectorMaterial;
        public bool preDrilled;
        public double pk1;
        public double pk2;
        public double t_head;
        public double t_thread;
        public double npar;
        public double npep;
        public string woodType;
        public double a1;

        public Ccalc_Variables variables;

        public Ccalc_T2TCapacity() { }

        //to shear connections
        public Ccalc_T2TCapacity(
            Ccalc_Fastener Fastener,
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
            double A1
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
            this.npar = Npar;
            this.npep = Npep;
            this.a1 = A1;
            this.variables = new Ccalc_Variables(Fastener, PreDrilled, Pk1, Pk2, Alfa1, Alfa2, alfafast, WoodType, T1, T2, T_thread);
        }

        public object FvkSingleShear()
        {
            double Mryk = this.variables.Myrk;
            double Fh1k = this.variables.fh1k;
            double Fh2k = this.variables.fh2k;
            double Beta = this.variables.beta;
            double Faxrk = this.variables.Faxrk;
            double Fvk;
            string failureMode = "";

            //1º modo (a)
            double Fvk1 =  Fh1k * t1 * this.fastener.d;
            Fvk = Fvk1;
            failureMode = "a";

            //2º modo (b)
            double Fvk2 =  Fh2k * t2 * this.fastener.d;
            if (Fvk > Fvk2)
            {
                Fvk = Fvk2;
                failureMode = "b";
            }

            //3º modo (c)
            double Fvk3 = 0;
            double Fyk3 = ((Fh1k * t1 * this.fastener.d) / (1 + Beta))
                * (Math.Sqrt(Beta + 2 * Math.Pow(Beta, 2) * (1 + (t2 / t1) + Math.Pow(t2 / t1, 2)) + Math.Pow(Beta, 3) * Math.Pow(t2 / t1, 2)) - Beta * (1 + (t2 / t1)));
            Fvk3 = this.Fvk(Fyk3, Faxrk, 1);
            if (Fvk > Fvk3)
            {
                Fvk = Fvk3;
                failureMode = "c";
            }

            //4º modo (d)
            double Fvk4 = 0;
            double Fyk4 = ((1.05 * Fh1k * t1 * this.fastener.d) / (2 + Beta))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + ((4 * Beta * (2 + Beta) * Mryk) / (Fh1k * Math.Pow(t1, 2) * this.fastener.d))) - Beta);
            Fvk4 = this.Fvk(Fyk4, Faxrk, 1);
            if (Fvk > Fvk4)
            {
                Fvk = Fvk4;
                failureMode = "d";
            }

            //5º modo (e)
            double Fvk5 = 0;
            double Fyk5 = ((1.05 * Fh2k * t2 * this.fastener.d) / (2 + Beta))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + ((4 * Beta * (2 + Beta) * Mryk) / (Fh2k * Math.Pow(t2, 2) * this.fastener.d))) - Beta);
            Fvk5 = this.Fvk(Fyk5, Faxrk, 1);
            if (Fvk > Fvk5)
            {
                Fvk = Fvk5;
                failureMode = "e";
            }

            //6º modo (f)
            double Fvk6 = 0;
            double Fyk6 = 1.15 * Math.Sqrt((2 * Beta) / (1 + Beta))
                * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d);
            Fvk6 = this.Fvk(Fyk6, Faxrk, 1);
            if (Fvk > Fvk6)
            {
                Fvk = Fvk6;
                failureMode = "f";
            }

            return new
            {
                Fvrk = Fvk,
                failureMode
            };
        }

        public object FvkDoubleShear()
        {
            double Mryk = this.variables.Myrk;
            double Fh1k = this.variables.fh1k;
            double Fh2k = this.variables.fh2k;
            double Beta = this.variables.beta;
            double Faxrk = this.variables.Faxrk;
            double Fvk;
            string failureMode = "";

            // 1º mode (g)
            double Fvk1 =  (Fh1k * t1 * this.fastener.d);
            Fvk = Fvk1;
            failureMode = "g";

            // 2º mode (h)
            double Fvk2 =  (0.5 * Fh2k * t2 * this.fastener.d);
            if (Fvk > Fvk2)
            {
                Fvk = Fvk2;
                failureMode = "h";
            }

            // 3º mode (j)
            double Fvk3 = 0;
            double Fyk3 = (1.05 * ((Fh1k * t1 * this.fastener.d) / (2 * Beta)))
                * (Math.Sqrt(2 * Beta * (1 + Beta) + (4 * Beta * (2 + Beta) * Mryk) / (Fh1k * Math.Pow(t1, 2) * this.fastener.d)) - Beta);
            Fvk3 = this.Fvk(Fyk3, Faxrk, 1);
            if (Fvk > Fvk3)
            {
                Fvk = Fvk3;
                failureMode = "j";
            }

            // 4º mode (k)
            double Fvk4 = 0;
            double Fyk4 = (1.15 * Math.Sqrt((2 * Beta) / (1 + Beta)) * Math.Sqrt(2 * Mryk * Fh1k * this.fastener.d));
            Fvk4 = this.Fvk(Fyk4, Faxrk, 1);
            if (Fvk > Fvk4)
            {
                Fvk = Fvk4;
                failureMode = "k";
            }

            return new
            {
                Fvrk = Fvk,
                failureMode
            };
        }

        public double Nef()
        {
            double d = this.fastener.d;
            double nef = 0;
            if (this.fastener.type == "nail" || (this.fastener.type == "screw" & this.fastener.d < 6))
            {
                double kef = 0;
                if (this.a1 >= 4 * d & this.a1 < 7 * d)
                {
                    kef = 0.5 - (0.5 - 0.7) * (4 * d - a1) / (4 * d - 7 * d);
                }
                if (this.a1 >= 7 * d & this.a1 < 10 * d)
                {
                    kef = 0.7 - (0.7 - 0.85) * (7 * d - a1) / (7 * d - 10 * d);
                }
                if (this.a1 >= 10 * d & this.a1 < 14 * d)
                {
                    kef = 0.85 - (0.85 - 1) * (10 * d - a1) / (10 * d - 14 * d);
                }
                if (this.a1 >= 14 * d)
                {
                    kef = 1;
                }
                nef = (Math.Pow(npar, kef)) * npep;
            }
            if (this.fastener.type == "bolt" || (this.fastener.type == "screw" & this.fastener.d > 6))
            {
                nef = Math.Min(npar, Math.Pow(npar, 0.9) * Math.Pow(a1 / (13 * d), 0.25)) * npep;
            }
            return nef;
        }

        public double Fvk(double fyk, double faxrk, double nalfacrit)
        {
            double fvk = 0;
            if (this.fastener.type == "screw")
            {
                if (fyk < faxrk / 4)
                {
                    faxrk = 4 * fyk;
                }
                fvk = nalfacrit * (fyk + faxrk / 4);

            }
            if (this.fastener.type == "nail")
            {
                if (fyk < faxrk / 4)
                {
                    faxrk = 4 * 0.15 * fyk;
                }
                fvk = nalfacrit * (fyk + faxrk / 4);
            }
            if (this.fastener.type == "bolt")
            {
                if (fyk < faxrk / 4)
                {
                    faxrk = 4 * 0.25 * fyk;
                }
                fvk = nalfacrit * (fyk + faxrk / 4);
            }
            return fvk;
        }
    }
}
