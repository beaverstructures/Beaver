using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
namespace Beaver_v0._1
{
    class Ccalc_Variables
    {
        public double Myrk;
        public double fhk;
        public double fh1k;
        public double fh2k;
        public double beta;
        public double Faxrk;
        public double tpen;
        public string error;

        public Ccalc_Variables() { }

        public Ccalc_Variables( //For shear timber to timber cases
            Ccalc_Fastener fastener,
            bool preDrilled,
            double pk1,
            double pk2,
            double alfa1,
            double alfa2,
            double alfafast,
            string woodType,
            double t1,
            double t2,
            double t_thread
        )
        {
            this.Myrk = CalcMyrk(fastener);
            this.fh1k = CalcFhk(preDrilled, fastener, pk1, alfa1, woodType);
            this.fh2k = CalcFhk(preDrilled, fastener, pk2, alfa2, woodType);
            this.beta = this.fh2k / this.fh1k;
            this.tpen = GetTpen(fastener, t1, t2);
            this.Faxrk = CalcFaxrk(pk1, fastener, t1, this.tpen, alfafast, t_thread);
        }

        public Ccalc_Variables( //For timber to steel cases
            Ccalc_Fastener fastener,
            bool preDrilled,
            double pk,
            double alfa,
            double alfafast,
            string woodType,
            double t1,
            double t_steel,
            double t_thread
        )
        {
            this.Myrk = CalcMyrk(fastener);
            this.fhk = CalcFhk(preDrilled, fastener, pk, alfa, woodType);
            this.Faxrk = CalcFaxrk(pk, fastener, t1,t1- t_steel, alfafast, t_thread);
        }

        public double GetTpen(Ccalc_Fastener fastener, double t1, double t2)
        {
            double tpoint = fastener.l - t1;
            if (t2 - tpoint <= 4 * fastener.d)
            {
                this.error = "(t2 - tpoint) must be at least 4d";
            }
            else if (tpoint < 8 * fastener.d)
            {
                this.error = "tpoint must be at least 8d";
            }
            return tpoint;
        }

        public double CalcMyrk(Ccalc_Fastener fastener)
        {
            double value = 0;
            if (fastener.type == "nail" && fastener.smooth == true)
            {
                value = 0.3 * fastener.fu * Math.Pow(fastener.d, 2.6);
            }
            else if ((fastener.type == "nail" && fastener.smooth == false) || (fastener.type == "screw" && fastener.d <= 6))
            {
                value = 0.45 * fastener.fu * Math.Pow(fastener.d, 2.6);
            }
            else if (fastener.type == "bolt" || (fastener.type == "screw" && fastener.d > 6))
            {
                value = 0.3 * fastener.fu * Math.Pow(fastener.d, 2.6);
            }

            return value;
        }

        double CalcFhk(bool preDrilled, Ccalc_Fastener fastener, double pk, double alfa, string woodType)
        {
            double fhk = 0;
            if ((fastener.type == "nail" && fastener.d <= 8) || (fastener.type == "screw" && fastener.d <= 6))
            {
                if (preDrilled == false)
                {
                    fhk = 0.082 * pk * Math.Pow(fastener.d, -0.3);
                }
                else
                {
                    fhk = 0.082 * (1 - 0.01 * fastener.d) * pk;
                }
            }
            if ((fastener.type == "nail" && fastener.d > 8) || (fastener.type == "bolt") || (fastener.type == "screw" && fastener.d > 6))
            {
                fhk = CalculateFhAlfak(fastener.d, pk, alfa, woodType);
            }
            return fhk;
        }

        double CalculateFhAlfak(double d, double pk, double alfa, string woodType)
        {
            double f0hk = 0.082 * (1 - 0.01 * d) * pk;
            if (alfa == 0)
            {
                return f0hk;
            }
            else
            {
                double k90 = CalcK90(d, woodType);
                return f0hk / (k90 * Math.Pow(Math.Sin(alfa), 2) + Math.Pow(Math.Cos(alfa), 2));
            }

        }

        double CalcK90(double d, string woodType)
        {
            double k90 = 1.3 + 0.015 * d;
            if (woodType == "softwood")
            {
                k90 = (1.35 + 0.015 * d);
            }
            else if (woodType == "hardwood")
            {
                k90 = (0.9 + 0.015 * d);
            }
            else if (woodType == "lvl" || woodType == "mlc")
            {
                k90 = (1.3 + 0.015 * d);
            }
            return k90;
        }

        double CalcFaxrk(double pk, Ccalc_Fastener fastener, double t1, double tpen, double alfa, double t_thread)
        {
            double value = 0;
            if (fastener.type == "nail")
            {
                double fpaxk = CalcNailfaxk(tpen, fastener.d, pk, fastener.smooth);
                double fhaxk = CalcNailfaxk(t1, fastener.d, pk, fastener.smooth);
                double fheadk = CalcNailfheadk(pk);
                value = Math.Min(fpaxk * fastener.d * tpen, fhaxk * fastener.d * t1 + fheadk * Math.Pow(fastener.dh, 2));
            }
            else if (fastener.type == "screw")
            {
                value = CalcScrewFaxrk(fastener.d, pk, alfa, tpen, t_thread);
            }
            else if (fastener.type == "bolt")
            {
                double fc90k = t_thread;
                double aread = Math.Pow(fastener.d, 2) * Math.PI / 4;
                double areadw = Math.Pow(fastener.dh, 2) * Math.PI / 4;
                value = Math.Min(3 * fc90k * (areadw - aread), fastener.fu * aread);
            }


            return value;
        }

        double CalcNailfaxk(double tpen, double d, double pk, bool smooth)
        {
            double faxk = 20 * Math.Pow(10, -6) * Math.Pow(pk, 2);
            double coef = CalcFaxkCoeficient(d, tpen, smooth);
            return coef * faxk;
        }

        double CalcFaxkCoeficient(double d, double t, bool smooth)
        {
            double coef = 1;
            if (smooth == true)
            {
                if (t < 8 * d)
                {
                    coef = 0;
                }
                else if (t > 8 * d && t < 12 * d)
                {
                    coef = t / (4 * d - 2);
                }

                else
                {
                    if (t < 6 * d)
                    {
                        coef = 0;
                    }
                    else if (t > 6 * d && t < 8 * d)
                    {
                        coef = t / (2 * d - 3);
                    }
                }
            }
            return coef;
        }

        double CalcNailfheadk(double pk)
        {
            double fheadk = 70 * Math.Pow(10, -6) * Math.Pow(pk, 2);
            return fheadk;
        }

        double CalcScrewFaxrk(double d, double pk, double alfa, double tpen, double t_thread)
        {
            
            double l_ef2;
            if (tpen <= t_thread)
            {
                l_ef2 = tpen - d;
            }
            else
            {
                l_ef2 =t_thread - d;
            }
            double f_ax_k = 0.52 * Math.Pow(d, -0.5) * Math.Pow(l_ef2, -0.1) * Math.Pow(pk,0.8);
            double f_ax_alfa_k = f_ax_k / (Math.Pow(Math.Sin(alfa), 2) + 1.2 * Math.Pow(Math.Cos(alfa), 2));
            double F_1_ax_alfa_k = d * l_ef2 * f_ax_alfa_k * Math.Min(d / 8, 1) ;
            return F_1_ax_alfa_k;
        }
    }
}