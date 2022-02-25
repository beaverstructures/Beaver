using System;
using System.Collections.Generic;
using System.Text;
using BeaverCore.Actions;
using BeaverCore.CrossSection;
using BeaverCore.Misc;
using BeaverCore.Materials;
using BeaverCore.Geometry;

namespace BeaverCore.Frame
{
    [Serializable]
    public class TimberFramePoint
    {
        /// <summary>
        /// A referenced point on a TimberFrame element
        /// </summary>
        public List<Force> Forces;
        public List<Displacement> Disp;
        public ULSCombinations ULSComb;
        public SLSCombinations SLSComb;
        public CroSec CS;

        public bool cantilever;
        public double ly;
        public double lz;
        public double kflam;
        public double lspan;
        public double inst_deflection_limit;
        public double netfin_deflection_limit;
        public double fin_deflection_limit;
        public double precamber;
        public string id;
        public string guid;
        public string parameters;
        public int sc;

        // calculated displacement average of end points of span line for each load case. 
        // For now it is the same for all timber frame. 
        // In reality should be an interpolation of the reference line at the position of the timberframepoint
        public List<Displacement> localRefDisps;
        public SLSCombinations RefSLSComb;
        public bool local;

        public TimberFramePoint() { }

        public TimberFramePoint(List<Force> forces, List<Displacement> disp, CroSec cs, int sc, double ly, double lz, double lspan, double kflam, bool cantilever = false, double precamber = 0, bool local = false, List<Displacement> _localRefDisps = null)
        {
            Forces = forces;
            Disp = disp;
            localRefDisps = _localRefDisps;

            this.sc = sc;
            CS = cs;
            CS.material.Setkdef(sc);

            this.ly = ly;
            this.lz = lz;
            this.kflam = kflam;
            this.lspan = lspan;
            this.cantilever = cantilever;
            this.precamber = precamber;
            this.local = local;
            if (cantilever)
            {
                fin_deflection_limit = lspan / 150;
                inst_deflection_limit = lspan / 175;
                netfin_deflection_limit = lspan / 125;
            }
            else
            {
                fin_deflection_limit = lspan / 300;
                inst_deflection_limit = lspan / 350;
                netfin_deflection_limit = lspan / 250;
            }
            SetRefDisp();
            SetSLSComb();
            SetULSComb();
        }

        public void SetULSComb()
        {
            ULSComb = new ULSCombinations(Forces, sc);
        }

        public void SetSLSComb()
        {
            SLSComb = new SLSCombinations(Disp, sc, CS.material);
        }

        public void SetRefDisp()
        {
            if (local)
            {
                RefSLSComb = new SLSCombinations(localRefDisps, sc, CS.material);
            }
        }

        private double Getkcrit(double lam)
        {
            double kcrit = 1;
            if (lam >= 0.75 && lam < 1.4)
            {
                kcrit = 1.56 - 0.75 * lam;
            }
            else if (lam >= 1.4)
            {
                kcrit = 1 / Math.Pow(lam, 2);
            }
            return kcrit;
        }

        public TimberFrameULSResult ULSUtilization()
        {
            string[] Info = new string[] {
                    //0
                    "EC5 Section 6.1.2 Tension parallel to the grain",
                    //1
                    "EC5 Section 6.1.4 Compression parallel to the grain.",
                    //2
                    "EC5 Section 6.1.6 Biaxial Bending",
                    //3
                    "EC5 Section 6.1.7 Shear",
                    //4
                    "EC5 Section 6.1.8 Torsion",
                    //5
                    "EC5 Section 6.2.3 Combined Tension and Bending",
                    //6 
                    "EC5 Section 6.2.4 Combined Bending and Axial Compression",
                    //7
                    "EC5 Section 6.3.2 Columns subjected to either compression or combined compression and bending",
                    //8
                    "EC5 Section 6.3.3 Beams subjected to either bending or combined bending and compression",
                    //9
                    "Combined Torsion and Shear",
                };

            ///Basic Geometry and Material Values
            double A = CS.A;
            double Wy = CS.Wy;
            double Wz = CS.Wz;
            double ry = CS.ry;
            double rz = CS.rz;

            double Ym = CS.material.Ym;
            double fc0k = CS.material.fc0k;
            double ft0k = CS.material.ft0k;
            double fmk = CS.material.fmk;
            double Fvk = CS.material.fvk;
            double E05 = CS.material.E05;
            double G05 = CS.material.G05;
            double Bc = CS.material.Bc;


            ///Define EC5 Section 6.1.6 Bending Coefficients
            double Km = 1;
            if (CS is CroSec_Rect) { Km = 0.7; };
            if (CS is CroSec_Circ) { Km = 1; };
            
            ///Define EC5 Section 6.1.7 Shear Coefficients
            double kcrit = 0.67;

            ///Define EC5 Section 6.1.8 Torsion Coefficients
            double kshape = 2;
            CroSec_Rect cs = (CroSec_Rect)CS;
            if (CS is CroSec_Rect) { kshape = Math.Min(1 + 0.15 * (cs.h / cs.b), 2); };
            if (CS is CroSec_Circ) { kshape = 1.2; };

            ///Define EC5 Section 6.2.3 & 6.3.2 Coefficients
            double lefy = ly * kflam;
            double lefz = lz * kflam;
            double sigMcrity = CS.GetsigMcrit(lefy, E05, G05);
            double sigMcritz = CS.GetsigMcrit(lefz, E05, G05);
            double lammy = Math.Sqrt(fmk / sigMcrity);
            double lammz = Math.Sqrt(fmk / sigMcritz);
            double kcrity = Getkcrit(lammy);
            double kcritz = Getkcrit(lammz);

            ///Define EC5 Section 6.3.2 Coefficients
            double lamy = (ly) / ry;
            double lamz = (lz) / rz;
            double lampi = Math.Sqrt(fc0k / E05) / Math.PI;
            double lamyrel = lamy * lampi;
            double lamzrel = lamz * lampi;
            double ky = 0.5 * (1 + Bc * (lamyrel - 0.3) + Math.Pow(lamyrel, 2));
            double kz = 0.5 * (1 + Bc * (lamzrel - 0.3) + Math.Pow(lamzrel, 2));
            double kyc = 1 / (ky + Math.Sqrt(Math.Pow(ky, 2) - Math.Pow(lamyrel, 2)));
            double kzc = 1 / (kz + Math.Sqrt(Math.Pow(kz, 2) - Math.Pow(lamzrel, 2)));

            ///Define Basic Coefficients Output
            string Sectiondata = string.Format(
                "kflam = {0}, kcrit ={1}, kshape = {2}, λy ={3}, λz ={4},λrely ={5}, λrelz ={6}, ky ={7}, kz ={8}, kcy={9}, kcz={10}, σMcrity= {11}, σMcritz= {12}, λmy ={13}, λmz ={14}",
                    Math.Round(kflam, 3), Math.Round(kcrit, 3), Math.Round(kcrit, 3), Math.Round(lamy, 3), Math.Round(lamz, 3), Math.Round(lamyrel, 3), Math.Round(lamzrel, 3), Math.Round(ky, 3), Math.Round(kz, 3),
                    Math.Round(kyc, 3), Math.Round(kzc, 3), Math.Round(sigMcrity, 3), Math.Round(sigMcritz, 3), Math.Round(lammy, 3), Math.Round(lammz, 3));
            parameters = Sectiondata;

            ///Define Utilization
            double Util0;
            double Util1;
            double UtilY2;
            double UtilZ2;
            double UtilY3;
            double UtilZ3;
            double Util4;
            double UtilY5;
            double UtilZ5;
            double UtilY6;
            double UtilZ6;
            double UtilY7;
            double UtilZ7;
            double UtilY8;
            double UtilZ8;
            double Util9;
            List<double[]> AllUtilsY = new List<double[]>();
            List<double[]> AllUtilsZ = new List<double[]>();
            foreach (Force f in ULSComb.DesignForces)
            {
                ///Actions
                double Nd = f.N;
                double Vy = f.Vy;
                double Vz = f.Vz;
                double Myd = f.My;
                double Mzd = f.Mz;
                double Mt = f.Mt;
                double sigN = Nd / A;
                double Sigvy = (3 / 2) * (Vy / (kcrit * CS.A));
                double Sigvz = (3 / 2) * (Vz / (kcrit * CS.A));
                double sigMy = Math.Abs(Myd) / Wy;
                double sigMz = Math.Abs(Mzd) / Wz;
                double SigMt = Math.Abs(Mt) / CS.It;

                ///Resistances
                double Kmod = Utils.KMOD(ULSComb.SC, f.duration);
                double fc0d = Kmod * fc0k / Ym;
                double ft0d = Kmod * ft0k / Ym;
                double fvd = Kmod * Fvk / Ym;
                double fmd = Kmod * fmk / Ym;

                ///0 EC5 Section 6.1.2 Tension parallel to the grain
                if (sigN < 0) Util0 = 0;
                else Util0 = Math.Abs(sigN) / ft0d;

                ///1 EC5 Section 6.1.4 Compression parallel to the grain
                if (sigN > 0) Util1 = 0;
                else Util1 = Math.Abs(sigN) / fc0d;

                ///2 EC5 Section 6.1.6 Biaxial Bending
                UtilY2 = Math.Abs(sigMy / fmd) + Km * Math.Abs(sigMz / fmd);
                UtilZ2 = Km * Math.Abs(sigMy / fmd) + Math.Abs(sigMz / fmd);

                ///3 EC5 Section 6.1.7 Shear
                UtilY3 = Math.Abs(Sigvy) / fvd;
                UtilZ3 = Math.Abs(Sigvz) / fvd;

                ///4 EC5 Section 6.1.8 Torsion
                Util4 = Math.Abs(SigMt) / (kshape * fvd);

                ///9 Combined Shear and Tension - Not specified in EC5
                Util9 = Math.Max(UtilY3 + Util4, UtilZ3 + Util4);
                // Less conservative approach: 
                // Util9 = Math.Max(Math.Pow(UtilY3,2) + Util4, Math.Pow(UtilZ3,2) + Util4);

                ///5 EC5 Section 6.2.3 Combined Bending and Axial Tension
                if (sigN < 0) UtilY5 = UtilZ5 = 0;
                else
                {
                    UtilY5 = sigN / ft0d + Math.Abs(sigMy / fmd) + Km * Math.Abs(sigMz / fmd);
                    UtilZ5 = sigN / ft0d + Km * Math.Abs(sigMy / fmd) + Math.Abs(sigMz / fmd);
                }

                /// 6 EC5 Section 6.2.4 Combined Bending and Axial Compression
                if (sigN > 0) UtilY6 = UtilZ6 = 0;
                else
                {
                    UtilY6 = Math.Pow((Math.Abs(sigN) / fc0d), 2) + Math.Abs(sigMy / fmd) + Km * Math.Abs(sigMz / fmd);
                    UtilZ6 = Math.Pow((Math.Abs(sigN) / fc0d), 2) + Km * Math.Abs(sigMy / fmd) + Math.Abs(sigMz / fmd);
                }

                if(Util1 > Math.Max(UtilY2, UtilZ2))
                /// If compression is more critical than bending, treat it as a column
                {
                    ///7 EC5 Section 6.3.2 Columns subjected to either compression or combined compression and bending
                    if (lamyrel <= 0.3 && lamzrel <= 0.3)
                    {
                        UtilY7 = Math.Pow(sigN / fc0d, 2) + Math.Abs(sigMy / fmd) + Km * Math.Abs(sigMz / fmd);
                        UtilZ7 = Math.Pow(sigN / fc0d, 2) + Km * Math.Abs(sigMy / fmd) + Math.Abs(sigMz / fmd);
                    }
                    else
                    {
                        UtilY7 = Math.Abs(sigN / (kyc * fc0d)) + Math.Abs(sigMy / fmd) + Km * Math.Abs(sigMz / fmd);
                        UtilZ7 = Math.Abs(sigN / (kzc * fc0d)) + Km * Math.Abs(sigMy / fmd) + Math.Abs(sigMz / fmd);
                    }
                    UtilY8 = 0;
                    UtilZ8 = 0;
                }
                else
                {
                    ///8 EC5 Section 6.3.3 Beams subjected to either bending or combined bending and compression
                    if (Math.Max(lammy, lammz) >= 0.75 && Math.Max(lammy, lammz) < 1.4)
                    {
                        UtilY8 = Math.Pow(sigMy / (kcrity * fmd), 2) + Math.Abs(sigN / (kzc * fc0d)) + Km * Math.Abs(sigMz / fmd);
                        UtilZ8 = Math.Pow(sigMz / (kcritz * fmd), 2) + Math.Abs(sigN / (kyc * fc0d)) + Km * Math.Abs(sigMy / fmd); ;
                    }
                    if (Math.Max(lammy, lammz) >= 1.4)
                    {
                        UtilY8 = Math.Pow(sigMy / (kcrity * fmd), 2) + Math.Abs(sigN / (kzc * fc0d)) + Km * Math.Abs(sigMz / fmd);
                        UtilZ8 = Math.Pow(sigMz / (kcritz * fmd), 2) + Math.Abs(sigN / (kyc * fc0d)) + Km * Math.Abs(sigMy / fmd);
                    }
                    else
                    {
                        UtilY8 = 0;
                        UtilZ8 = 0;
                    }
                    UtilY7 = 0;
                    UtilZ7 = 0;
                }
                List<double> UtilsY = new List<double>() { Util0, Util1, UtilY2, UtilY3, Util4, UtilY5, UtilY6, UtilY7, UtilY8 , Util9 };
                List<double> UtilsZ = new List<double>() { Util0, Util1, UtilZ2, UtilZ3, Util4, UtilZ5, UtilZ6, UtilZ7, UtilZ8 , Util9 };
                AllUtilsY.Add(UtilsY.ToArray());
                AllUtilsZ.Add(UtilsZ.ToArray());

            }

            TimberFrameULSResult Result = new TimberFrameULSResult(Info, AllUtilsY, AllUtilsZ, Sectiondata);

            return Result;

        }

        public TimberFrameSLSResult SLSUtilization()
        {
            string[] Info = new string[] {
                    //0
                    "EC5 Section 7.2 Instantaneous deflection",
                    //1
                    "EC5 Section 7.2 Net final deflection",
                    //2
                    "EC5 Section 7.2 Final deflection",
                };
            return new TimberFrameSLSResult(Info, InstDisplacementUtil(), NetFinDisplacementUtil(), FinDisplacementUtil());
        }


        public List<double> InstDisplacementUtil()
        {
            double deflection_limit = inst_deflection_limit;
            List<double> disps_ratio = new List<double>();
            int i = 0;
            foreach (Displacement disp in SLSComb.CharacteristicDisplacements)
            {
                if (local)
                {
                    /// Local displacements
                    Displacement localDisp = disp - RefSLSComb.CharacteristicDisplacements[i];
                    disps_ratio.Add(localDisp.Absolute() / deflection_limit);
                }
                else
                {
                    /// Global Displacements
                    disps_ratio.Add(disp.Absolute() / deflection_limit);
                }
                i++;
            }
            return disps_ratio;
        }

        public List<double> NetFinDisplacementUtil()
        {
            double deflection_limit = netfin_deflection_limit;
            List<double> disps_ratio = new List<double>();
            int i = 0;
            foreach (var disp in SLSComb.CreepDisplacements)
            {
                if (local)
                {
                    /// Local displacements
                    Displacement localDisp = disp - RefSLSComb.CreepDisplacements[i];
                    disps_ratio.Add(localDisp.Absolute() / deflection_limit);
                }
                else
                {
                    /// Global Displacements
                    disps_ratio.Add(disp.Absolute() / deflection_limit);
                }
                i++;
            }
            return disps_ratio;
        }

        public List<double> FinDisplacementUtil()
        {
            double deflection_limit = fin_deflection_limit;
            List<double> disps_ratio = new List<double>();
            int i = 0;
            foreach (var disp in SLSComb.CreepDisplacements)
            {
                if (local)
                {
                    /// Local displacements
                    Displacement localDisp = disp - RefSLSComb.CharacteristicDisplacements[i];
                    disps_ratio.Add(localDisp.Absolute() / deflection_limit);
                }
                else
                {
                    /// Global Displacements
                    disps_ratio.Add(disp.Absolute() / deflection_limit);
                }
                i++;
            }
            return disps_ratio;
        }

    }
}
