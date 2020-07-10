using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaver_v0._1
{
    class Ccalc_T2SCapacity
    {
        public Ccalc_Variables variables;
        public Ccalc_Fastener fastener;
        public bool preDrilled;
        public double pk;
        public double alfa;
        public double alfafast;
        public string woodType;
        public double t1;
        public double t_steel;
        public double t_thread;
        public double npar;
        public double npep;
        public double Faxrk_upperLimit;
        public int SDt;
        public string failureMode = "";
        public double a1;

        public Ccalc_T2SCapacity() { }

        public Ccalc_T2SCapacity(
            Ccalc_Fastener Fastener,
            bool PreDrilled,
            double Pk,
            double Alfa,
            double Alfafast,
            string WoodType,
            double T1,
            double T_steel,
            double T_thread,
            double Npar,
            double Npep,
            int SD,
            double A1
        )
        {
            this.fastener = Fastener;
            this.preDrilled = PreDrilled;
            this.pk = Pk;
            this.alfa = Alfa;
            this.woodType = WoodType;
            this.t1 = T1;
            this.t_steel = T_steel;
            this.npar = Npar;
            this.npep = Npep;
            this.SDt = SD;
            this.variables = new Ccalc_Variables(Fastener, PreDrilled, Pk, Alfa, Alfafast, woodType, T1, T_steel, T_thread);
            this.Faxrk_upperLimit = this.CalcFaxrkUpperLimitValue(Fastener);
            this.a1 = A1;
            this.alfafast=Alfafast;
        }

        private double CalcFaxrkUpperLimitValue(Ccalc_Fastener fastener)
        {
            string type = fastener.type;

            if (type == "nail")
            {
                return 0.15;
            }

            else if (type == "screw")
            {
                return 1;
            }
            else if (type == "bolt")
            {
                return 0.25;
            }
            else if (type == "dowel")
            {
                return 0;
            }
            return 1;
        }

        public object FvrkSingleShear()
        {

            double Fvrk;
            //Mode a
            double Fvrk1 =  0.4 * variables.fhk * t1 * fastener.d;
            Fvrk = Fvrk1;
            string failureMode = "a";

            //Mode b
            double Fyrk2 = (1.15 * Math.Sqrt(2 * variables.Myrk * variables.fhk * fastener.d));
            double Fvrk2 = Math.Min(Fyrk2 + variables.Faxrk / 4, (1 + this.Faxrk_upperLimit) * Fyrk2);
            if (Fvrk > Fvrk2)
            {
                Fvrk = Fvrk2;
                failureMode = "b";
            }

            //Mode c
            double Fyrk3 = (variables.fhk * t1 * fastener.d * (Math.Sqrt(2 + (4 * variables.Myrk) / (variables.fhk * Math.Pow(t1, 2) * fastener.d)) - 1));
            double Fvrk3 = Math.Min(Fyrk3 + variables.Faxrk / 4, (1 + this.Faxrk_upperLimit) * Fyrk3);
            if (Fvrk > Fvrk3)
            {
                Fvrk = Fvrk3;
                failureMode = "c";
            }

            //Mode d
            double Fyrk4 = (2.3 * Math.Sqrt(variables.Myrk * variables.fhk * fastener.d));
            double Fvrk4 =  Math.Min(Fyrk4 + variables.Faxrk / 4, (1 + this.Faxrk_upperLimit) * Fyrk4);
            if (Fvrk > Fvrk4)
            {
                Fvrk = Fvrk4;
                failureMode = "d";
            }

            //Mode e
            double Fvrk5 = variables.fhk * t1 * fastener.d;
            if (Fvrk > Fvrk5)
            {
                Fvrk = Fvrk5;
                failureMode = "e";
            }
            return new
            {
                Fvrk,
                failureMode
            };
        }

        public object FvrkDoubleShear()
        {

            double Fvrk = 0;
            string failureMode = "";
            if (SDt == 1)
            {
                //Mode f
                double Fvrk1 = variables.fhk * t1 * fastener.d;
                Fvrk = Fvrk1;
                failureMode = "f";

                //Mode g
                double Fyrk2 = (variables.fhk * t1 * fastener.d * (Math.Sqrt(2 + (4 * variables.Myrk) / (variables.fhk * Math.Pow(t1, 2) * fastener.d)) - 1));
                double Fvrk2 =Math.Min(Fyrk2 + variables.Faxrk / 4, (1 + this.Faxrk_upperLimit) * Fyrk2);
                if (Fvrk > Fvrk2)
                {
                    Fvrk = Fvrk2;
                    failureMode = "g";
                }

                //Mode h
                double Fyrk3 = (2.3 * Math.Sqrt(variables.Myrk * variables.fhk * fastener.d));
                double Fvrk3 = Math.Min(Fyrk3 + variables.Faxrk / 4, (1 + this.Faxrk_upperLimit) * Fyrk3);
                if (Fvrk > Fvrk3)
                {
                    Fvrk = Fvrk3;
                    failureMode = "h";
                }
            }

            else if (SDt == 2)
            {
                //Mode j/l
                double Fvrk4 = 0.5 * variables.fhk * fastener.d * t1;
                Fvrk = Fvrk4;
                failureMode = "j/l";

                double multi = 0;
                if (this.t_steel <= 0.5 * this.fastener.d)
                {
                    multi = 1.62;
                }
                else if (this.t_steel >= this.fastener.d)
                {
                    multi = 2.3;
                }
                else
                {
                    multi = 2.3 - 1.36 * (this.fastener.d - this.t_steel) / this.fastener.d;
                }
                //Mode k/m
                double Fyrk6 = (multi * Math.Sqrt(variables.Myrk * variables.fhk * fastener.d));
                double Fvrk6 = Math.Min(Fyrk6 + variables.Faxrk / 4, (1 + this.Faxrk_upperLimit) * Fyrk6);
                if (Fvrk > Fvrk6)
                {
                    Fvrk = Fvrk6;
                    failureMode = "k/m";
                }
            }


            return new
            {
                Fvrk,
                failureMode
            };
        }

        

    }
}
