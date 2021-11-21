using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Connections
{


    public abstract class SingleFastenerCapacity
    {
        // THIS IS AN ABSTRACT CLASS WITH CONSTRUCTORS ON T2T AND S2T
        // CLASS HOSTING AXIAL AND SHEAR CAPACITIES ACCORDING TO FASTENER TYPE AND PENETRATIONS ON TIMBER FRAME

        // DEFINE VARIABLES
        public Fastener fastener;
        bool optionalPreDrilling;

        public int sheartype; //1 for single shear, 2 for double shear
        public Dictionary<string, double> shear_capacities;
        public double shear_crictical_capacity;
        public string shear_critical_failure_mode;

        public Dictionary<string, double> axial_capacities;
        public double axial_crictical_capacity;
        public string axial_critical_failure_mode;
        public bool rope_effect;

        public string analysisType;

        public double Myrk;
        public double fhk;
        public double fh1k;
        public double fh2k;
        public double beta;
        public double Faxrk;
        public double tpen;
        public string error = null;

        public double kser;

        public SingleFastenerCapacity() { }

        public enum ConnectionType
        {
            TimbertoTimber,
            TimbertoSteel
        }

        // CONSTRUCTORS

        public void GetFvk()
        {
            if (sheartype == 1) shear_capacities = FvkSingleShear();
            else if (sheartype == 2) shear_capacities = FvkDoubleShear();
        }

        // ABSTRACT VARIABLES FOR CAPACITY CALCULATION ACCORDING TO CONNECTION SETUP

        public abstract Dictionary<string, double> FvkSingleShear();

        public abstract Dictionary<string, double> FvkDoubleShear();

        public abstract Dictionary<string, double> Faxk();

        // METHODS FOR CALCULATING SINGLE FASTENER CAPACITY

        public double FaxrkUpperLimitValue()
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

        public static double CalcKser(Fastener fastener, Material mat1, Material mat2)
        {
            // calculates Kser for the fastener
            // EC5 SECTION 7.1 TABLE 7.1
            double kser;
            double pm = Math.Sqrt(mat1.pk * mat2.pk);
            switch (fastener.type)
            {
                case "Dowel":
                    kser = Math.Pow(pm, 1.5) * Math.Pow(fastener.d, 0.8) / 23;
                    break;
                case "Nail":
                    kser = fastener.predrilled1 ?
                        Math.Pow(pm, 1.5) * Math.Pow(fastener.d, 0.8) / 23
                        : Math.Pow(pm, 1.5) * Math.Pow(fastener.d, 0.8) / 30;
                    break;
                case "Screw":
                    kser = Math.Pow(pm, 1.5) * Math.Pow(fastener.d, 0.8) / 23;
                    break;
                case "Staples":
                    kser = Math.Pow(pm, 1.5) * Math.Pow(fastener.d, 0.8) / 80;
                    break;
                default:
                    throw new ArgumentException("Could not find connection type");
            }
            return kser;
        }

        public void SetCriticalCapacity()
        {
            /// Finds and updates the critical capacity from the shear_capacities variable
            shear_crictical_capacity = 9999999;
            axial_crictical_capacity = 9999999;
            foreach (var keyValuePair in shear_capacities)
            {
                if (keyValuePair.Value < shear_crictical_capacity)
                {
                    shear_crictical_capacity = keyValuePair.Value;
                    shear_critical_failure_mode = keyValuePair.Key;
                }
            }
            foreach (var keyValuePair in axial_capacities)
            {
                if (keyValuePair.Value < axial_crictical_capacity)
                {
                    axial_crictical_capacity = keyValuePair.Value;
                    axial_critical_failure_mode = keyValuePair.Key;
                }
            }
        }

        public double GetTpen(Fastener fastener, double t1, double t2)
        {
            double tpoint = fastener.l - t1;
            if (t2 - tpoint <= 4 * fastener.d)
            {
                this.error += "(t2 - tpoint) must be at least 4d";
            }
            else if (tpoint < 8 * fastener.d)
            {
                this.error += "tpoint must be at least 8d";
            }
            return tpoint;
        }

        public double CalcMyrk(Fastener fastener)
        {
            double value;
            switch (fastener.type)
            {
                case "Nail":
                    // EC5 SECTION 8.3.1.1 EQ 8.14
                    value = fastener.smooth ?
                        0.3 * fastener.fu * Math.Pow(fastener.d, 2.6)
                        : 0.45 * fastener.fu * Math.Pow(fastener.d, 2.6);
                    break;
                case "Screw":
                    value = fastener.d <= 6 ?
                        0.45 * fastener.fu * Math.Pow(fastener.d, 2.6)
                        : 0.3 * fastener.fu * Math.Pow(fastener.d, 2.6);
                    break;
                case "Bolt":
                    // EC5 SECTION 8.5.1.1 EQ 8.30
                    value = 0.3 * fastener.fu * Math.Pow(fastener.d, 2.6);
                    break;
                case "Staple":
                    value = 240 * Math.Pow(fastener.d, 2.6);
                    break;
                default:
                    throw new ArgumentException();
            }
            return value;
        }

        public static double CalcFhk(bool preDrilled, Fastener fastener, double pk, double alpha, string woodType)
        {
            double fhk = 0;
            double f0hk;
            double k90 = 0;

            switch (fastener.type)
            {
                case "Nail":
                    //  EC5 SECTION 8.3.1
                    if (fastener.d <= 8)
                    {
                        //  EC5 SECTION 8.3.1 EQ. 8.15
                        fhk = preDrilled ?
                                0.082 * pk * Math.Pow(fastener.d, -0.3)
                                : 0.082 * (1 - 0.01 * fastener.d) * pk;
                    }
                    else
                    {
                        // EC5 SECTION 8.3.1.1 clause 6 - TREAT IT AS A BOLT
                        switch (woodType)
                        {
                            // EC5 SECTION 8.5.1 EQ 8.31
                            case "Sofwood":
                                k90 = 1.35 + 0.015 * fastener.d; break;
                            case "Hardwood":
                                k90 = 0.9 + 0.015 * fastener.d; break;
                            case "LVL":
                            case "Gluelam":
                                k90 = 1.3 + 0.015 * fastener.d; break;
                            case "Plywood":
                                // EC5 SECTION 8.5.1 EQ 8.36
                                fhk = 0.11 * (1 - 0.01 * fastener.d); break;
                            case "OSB":
                                // EC5 SECTION 8.5.1 EQ 8.37
                                fhk = 0.50 * Math.Pow(fastener.d, -0.6) * Math.Pow(fastener.t, 0.2); break;
                            default:
                                throw new ArgumentException("Fhk not specified for this material");
                        }
                        if (k90 != 0)
                        {
                            f0hk = 0.082 * (1 - 0.01 * fastener.d) * pk;
                            fhk = f0hk / (k90 * Math.Pow(Math.Sin(alpha), 2) + Math.Pow(Math.Cos(alpha), 2));
                        }
                        break;
                    }
                    break;
                case "Staple":
                    //  EC5 SECTION 8.4 REFER TO SECTION 8.3
                    if (fastener.d <= 8)
                    {
                        //  EC5 SECTION 8.3.1 EQ. 8.15
                        fhk = preDrilled ?
                                0.082 * pk * Math.Pow(fastener.d, -0.3)
                                : 0.082 * (1 - 0.01 * fastener.d) * pk;
                    }
                    else
                    {
                        throw new ArgumentException("Fhk not specified for this diameter");
                    }
                    break;
                case "Bolt":
                    // EC5 SECTION 8.5.1
                    if (fastener.d < 30)
                    {
                        switch (woodType)
                        {
                            // EC5 SECTION 8.5.1 EQ 8.31
                            case "Sofwood":
                                k90 = 1.35 + 0.015 * fastener.d; break;
                            case "Hardwood":
                                k90 = 0.9 + 0.015 * fastener.d; break;
                            case "LVL":
                            case "Gluelam":
                                k90 = 1.3 + 0.015 * fastener.d; break;
                            case "Plywood":
                                // EC5 SECTION 8.5.1 EQ 8.36
                                fhk = 0.11 * (1 - 0.01 * fastener.d); break;
                            case "OSB":
                                // EC5 SECTION 8.5.1 EQ 8.37
                                fhk = 0.50 * Math.Pow(fastener.d, -0.6) * Math.Pow(fastener.t, 0.2); break;
                            default:
                                throw new ArgumentException("Fhk not specified for this material");
                        }
                        if (k90 != 0)
                        {
                            f0hk = 0.082 * (1 - 0.01 * fastener.d) * pk;
                            fhk = f0hk / (k90 * Math.Pow(Math.Sin(alpha), 2) + Math.Pow(Math.Cos(alpha), 2));
                        }
                        break;
                    }
                    else
                    {
                        throw new ArgumentException("Bolt diameter is too big");
                    }
                case "Dowel":
                    //  EC5 SECTION 8.6 REFER TO SECTION 8.5.1
                    if (6 < fastener.d && fastener.d < 30)
                    {
                        switch (woodType)
                        {
                            // EC5 SECTION 8.5.1 EQ 8.31
                            case "Sofwood":
                                k90 = 1.35 + 0.015 * fastener.d; break;
                            case "Hardwood":
                                k90 = 0.9 + 0.015 * fastener.d; break;
                            case "LVL":
                            case "Gluelam":
                                k90 = 1.3 + 0.015 * fastener.d; break;
                            case "Plywood":
                                // EC5 SECTION 8.5.1 EQ 8.36
                                fhk = 0.11 * (1 - 0.01 * fastener.d); break;
                            case "OSB":
                                // EC5 SECTION 8.5.1 EQ 8.37
                                fhk = 0.50 * Math.Pow(fastener.d, -0.6) * Math.Pow(fastener.t, 0.2); break;
                            default:
                                throw new ArgumentException("Fhk not specified for this material");
                        }
                        if (k90 != 0)
                        {
                            f0hk = 0.082 * (1 - 0.01 * fastener.d) * pk;
                            fhk = f0hk / (k90 * Math.Pow(Math.Sin(alpha), 2) + Math.Pow(Math.Cos(alpha), 2));
                        }
                        break;
                    }
                    else
                    {
                        throw new ArgumentException("Dowel diameter is not valid");
                    }
                case "Screw":
                    // EC5 SECTION 8.7.1
                    if (fastener.d > 6)
                    {
                        // REFER TO EC5 SECTION 8.5.1 - TREAT IT AS A BOLT
                        if (fastener.d < 30)
                        {
                            switch (woodType)
                            {
                                // EC5 SECTION 8.5.1 EQ 8.31
                                case "Sofwood":
                                    k90 = 1.35 + 0.015 * fastener.d; break;
                                case "Hardwood":
                                    k90 = 0.9 + 0.015 * fastener.d; break;
                                case "LVL":
                                case "Gluelam":
                                    k90 = 1.3 + 0.015 * fastener.d; break;
                                case "Plywood":
                                    // EC5 SECTION 8.5.1 EQ 8.36
                                    fhk = 0.11 * (1 - 0.01 * fastener.d); break;
                                case "OSB":
                                    // EC5 SECTION 8.5.1 EQ 8.37
                                    fhk = 0.50 * Math.Pow(fastener.d, -0.6) * Math.Pow(fastener.t, 0.2); break;
                                default:
                                    throw new ArgumentException("Fhk not specified for this material");
                            }
                            if (k90 != 0)
                            {
                                f0hk = 0.082 * (1 - 0.01 * fastener.d) * pk;
                                fhk = f0hk / (k90 * Math.Pow(Math.Sin(alpha), 2) + Math.Pow(Math.Cos(alpha), 2));
                            }
                            break;
                        }
                        else
                        {
                            throw new ArgumentException("Screw diameter is too big");
                        }
                    }
                    else
                    {
                        // REFER TO EC5 SECTION 8.3.1 - TREAT IT AS A NAIL
                        //  EC5 SECTION 8.3.1 EQ. 8.15
                        fhk = preDrilled ?
                                    0.082 * pk * Math.Pow(fastener.d, -0.3)
                                    : 0.082 * (1 - 0.01 * fastener.d) * pk;
                        break;
                    }

                default:
                    throw new ArgumentException("Could not find Fhk");
            }
            return fhk;
        }

        public static double CalcFtens(double ds, double fu, double Ymsteel)
        {
            return (Math.PI * Math.Pow(ds, 2) / 4) * fu / Ymsteel;
        }


        public static double CalcFaxrk(double pk, Fastener fastener, double t1, double tpen, double alpha, double t_thread)
        {
            double value = 0;
            if (fastener.type == "Nail")
            {
                double faxrk;
                // EC5 SECTION 8.3.2
                // EQ 8.25
                // EC5 SECTION 8.3.2 EQ. 8.23 fpaxk
                double faxk = 20 * Math.Pow(10, -6) * Math.Pow(pk, 2);
                double coef = 1;
                if (fastener.smooth == true)
                {
                    if (tpen < 8 * fastener.d)
                    {
                        coef = 0;
                    }
                    else if (tpen > 8 * fastener.d && tpen < 12 * fastener.d)
                    {
                        coef = tpen / (4 * fastener.d - 2);
                    }

                    else
                    {
                        if (tpen < 6 * fastener.d)
                        {
                            coef = 0;
                        }
                        else if (tpen > 6 * fastener.d && tpen < 8 * fastener.d)
                        {
                            coef = tpen / (2 * fastener.d - 3);
                        }
                    }
                }
                double fpaxk = coef * faxk;
                // EQ 8.26 fhaxk
                if (fastener.smooth == true)
                {
                    if (t1 < 8 * fastener.d)
                    {
                        coef = 0;
                    }
                    else if (t1 > 8 * fastener.d && t1 < 12 * fastener.d)
                    {
                        coef = t1 / (4 * fastener.d - 2);
                    }

                    else
                    {
                        if (t1 < 6 * fastener.d)
                        {
                            coef = 0;
                        }
                        else if (t1 > 6 * fastener.d && t1 < 8 * fastener.d)
                        {
                            coef = t1 / (2 * fastener.d - 3);
                        }
                    }
                }
                double fhaxk = coef * faxk;
                double fheadk = 70 * Math.Pow(10, -6) * Math.Pow(pk, 2);

                faxrk = Math.Min(
                    fpaxk * fastener.d * tpen,
                    fhaxk * fastener.d * t1 + fheadk * Math.Pow(fastener.dh, 2));
                if (8 * fastener.d < tpen && tpen < 12)
                {
                    return faxrk;
                }
                else
                {
                    return faxrk * (tpen / (2 * fastener.d) - 3);
                }
            }
            else if (fastener.type == "Screw")
            {
                // EC5 SECTION 8.7.2
                bool condition1 = fastener.d > 6 || fastener.d < 12;
                bool condition2 = fastener.d / fastener.ds > 0.6 || fastener.d / fastener.ds < 0.75;
                bool SECTION_8_7_2_item4 = condition1 && condition2;
                double nef = 1; // must be accounted later in AxialConnection.cs

                if (SECTION_8_7_2_item4)
                {
                    double l_ef2 = tpen <= t_thread ? (tpen - fastener.d) : (t_thread - fastener.d);
                    double f_ax_k = 0.52 * Math.Pow(fastener.d, -0.5) * Math.Pow(l_ef2, -0.1) * Math.Pow(pk, 0.8);      // EQ 8.39
                    double f_ax_alpha_k = f_ax_k / (Math.Pow(Math.Sin(alpha), 2) + 1.2 * Math.Pow(Math.Cos(alpha), 2)); // EQ 8.40
                    double F_1_ax_alpha_k = nef*fastener.d * l_ef2 * f_ax_alpha_k * Math.Min(fastener.d / 8, 1);        // EQ 8.38
                    value = F_1_ax_alpha_k;
                }
                else
                {
                    double l_ef2 = tpen <= t_thread ? (tpen - fastener.d) : (t_thread - fastener.d);
                    double f_ax_k = 0.52 * Math.Pow(fastener.d, -0.5) * Math.Pow(l_ef2, -0.1) * Math.Pow(pk, 0.8);      // EQ 8.39
                    double f_ax_alpha_k = f_ax_k / (Math.Pow(Math.Sin(alpha), 2) + 1.2 * Math.Pow(Math.Cos(alpha), 2)); // EQ 8.40
                    double F_1_ax_alpha_k = nef*fastener.d * l_ef2 * f_ax_alpha_k * Math.Pow(pk/385,0.8);               // EQ 8.40a // 385= rho a
                    value = F_1_ax_alpha_k;
                }
            }
            // EC5 SECTION 8.5.2
            else if (fastener.type == "Bolt")
            {
                double fc90k = t_thread;
                double aread = Math.Pow(fastener.d, 2) * Math.PI / 4;
                double areadw = Math.Pow(fastener.dh, 2) * Math.PI / 4;
                value = Math.Min(3 * fc90k * (areadw - aread), fastener.fu * aread);
            }
            return value;
        }
    }
}