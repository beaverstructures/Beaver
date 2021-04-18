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
        public int sheartype;

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
            this.alfafast=Alfafast;
            if (base.sheartype == 1) capacities = FvkSingleShear();
            else if (base.sheartype == 2) capacities = FvkDoubleShear();
            else { throw new ArgumentException(); }
        }

        
        public override if (base.sheartype == 1) capacity = FvkSingleShear(type);
            else if (base.sheartype == 2) capacity = FvkDoubleShear();
            else { throw new ArgumentException(); } GetFvk()
        {
            if (base.sheartype == 1) capacity = FvkSingleShear(type);
            else if (base.sheartype == 2) capacity = FvkDoubleShear();
            else { throw new ArgumentException(); }
        }
        public override object FvkSingleShear(bool type)
        {

            double maxFaxrk = this.FaxrkUpperLimitValue();
            double Fvrk =0;
            List<double> Fvrks = new List<double>();
            List<string> failures = new List<string>();
            //Mode a
            double Fvrk1 =  0.4 * variables.fhk * t1 * fastener.d;
            Fvrk = Fvrk1;
            string failureMode = "a";
            Fvrks.Add(Fvrk1);
            failures.Add("a");
            //Mode b
            double Fyrk2 = (1.15 * Math.Sqrt(2 * variables.Myrk * variables.fhk * fastener.d));
            double Fvrk2 = Math.Min(Fyrk2 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyrk2);
            Fvrks.Add(Fvrk2);
            failures.Add("b");

            if (Fvrk > Fvrk2)
            {
                Fvrk = Fvrk2;
                failureMode = "b";
            }

            //Mode c
            double Fyrk3 = (variables.fhk * t1 * fastener.d * (Math.Sqrt(2 + (4 * variables.Myrk) / (variables.fhk * Math.Pow(t1, 2) * fastener.d)) - 1));
            double Fvrk3 = Math.Min(Fyrk3 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyrk3);
            Fvrks.Add(Fvrk3);
            failures.Add("c");

            if (Fvrk > Fvrk3)
            {
                Fvrk = Fvrk3;
                failureMode = "c";
            }

            //Mode d
            double Fyrk4 = (2.3 * Math.Sqrt(variables.Myrk * variables.fhk * fastener.d));
            double Fvrk4 =  Math.Min(Fyrk4 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyrk4);
            Fvrks.Add(Fvrk4);
            failures.Add("d");
            if (Fvrk > Fvrk4)
            {
                Fvrk = Fvrk4;
                failureMode = "d";
            }

            //Mode e
            double Fvrk5 = variables.fhk * t1 * fastener.d;
            Fvrks.Add(Fvrk5);
            failures.Add("e");
            if (Fvrk > Fvrk5)
            {
                Fvrk = Fvrk5;
                failureMode = "e";
            }
            if (type)
            {
                return new
                {
                    Fvrk,
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

        public override object FvkDoubleShear()
        {
            double maxFaxrk = this.FaxrkUpperLimitValue();
            double Fvrk = 0;
            string failureMode = "";
            List<double> Fvrks = new List<double>();
            List<string> failures = new List<string>();
            if (sheartype == 1) //Double Shear Steel Out
            {
                //Mode f
                double Fvrk1 = variables.fhk * t1 * fastener.d;
                Fvrk = Fvrk1;
                failureMode = "f";
                Fvrks.Add(Fvrk1);
                failures.Add("f");
                //Mode g
                double Fyrk2 = (variables.fhk * t1 * fastener.d * (Math.Sqrt(2 + (4 * variables.Myrk) / (variables.fhk * Math.Pow(t1, 2) * fastener.d)) - 1));
                double Fvrk2 =Math.Min(Fyrk2 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyrk2);
                Fvrks.Add(Fvrk2);
                failures.Add("g");
                if (Fvrk > Fvrk2)
                {
                    Fvrk = Fvrk2;
                    failureMode = "g";
                }

                //Mode h
                double Fyrk3 = (2.3 * Math.Sqrt(variables.Myrk * variables.fhk * fastener.d));
                double Fvrk3 = Math.Min(Fyrk3 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyrk3);
                Fvrks.Add(Fvrk3);
                failures.Add("h");
                if (Fvrk > Fvrk3)
                {
                    Fvrk = Fvrk3;
                    failureMode = "h";
                }
            }

            else if (sheartype == 2) //Double Shear Steel In
            {
                //Mode j/l
                double Fvrk4 = 0.5 * variables.fhk * fastener.d * t1;
                Fvrk = Fvrk4;
                failureMode = "j/l";
                Fvrks.Add(Fvrk4);
                failures.Add("j/l");

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
                double Fvrk6 = Math.Min(Fyrk6 + variables.Faxrk / 4, (1 + maxFaxrk) * Fyrk6);
                Fvrks.Add(Fvrk6);
                failures.Add("k/m");
                if (Fvrk > Fvrk6)
                {
                    Fvrk = Fvrk6;
                    failureMode = "k/m";
                }
            }


            if (type)
            {
                return new
                {
                    Fvrk,
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
