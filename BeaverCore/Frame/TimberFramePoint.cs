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
        public Point3D pt;
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

        public double util;
        public int util_index;

        // calculated displacement average of end points of span line for each load case. 
        // For now it is the same for all timber frame. 
        // In reality should be an interpolation of the reference line at the position of the timberframepoint
        public List<Displacement> localRefDisps;
        public SLSCombinations RefSLSComb;
        public bool local;

        public TimberFramePoint() { }

        public TimberFramePoint(Point3D pt, List<Force> forces, List<Displacement> disp, CroSec cs, int sc, double ly, double lz, double lspan, double kflam, bool cantilever = false, double precamber = 0, bool local = false, List<Displacement> _localRefDisps = null)
        {
            this.pt = pt;
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
            double lamy = ly / ry;
            double lamz = lz / rz;
            double lampi = Math.Sqrt(fc0k / E05) / Math.PI;
            double lamyrel = lamy * lampi;
            double lamzrel = lamz * lampi;
            double ky = 0.5 * (1 + Bc * (lamyrel - 0.3) + Math.Pow(lamyrel, 2));
            double kz = 0.5 * (1 + Bc * (lamzrel - 0.3) + Math.Pow(lamzrel, 2));
            double kyc = 1 / (ky + Math.Sqrt(Math.Pow(ky, 2) - Math.Pow(lamyrel, 2)));
            double kzc = 1 / (kz + Math.Sqrt(Math.Pow(kz, 2) - Math.Pow(lamzrel, 2)));

            /*
            ///Define Basic Coefficients Output
            string Sectiondata = string.Format(
                "kflam = {0}, kcrit ={1}, kshape = {2}, λy ={3}, λz ={4},λrely ={5}, λrelz ={6}, ky ={7}, kz ={8}, kcy={9}, kcz={10}, σMcrity= {11}, σMcritz= {12}, λmy ={13}, λmz ={14}",
                    Math.Round(kflam, 3), Math.Round(kcrit, 3), Math.Round(kcrit, 3), Math.Round(lamy, 3), Math.Round(lamz, 3), Math.Round(lamyrel, 3), Math.Round(lamzrel, 3), Math.Round(ky, 3), Math.Round(kz, 3),
                    Math.Round(kyc, 3), Math.Round(kzc, 3), Math.Round(sigMcrity, 3), Math.Round(sigMcritz, 3), Math.Round(lammy, 3), Math.Round(lammz, 3));
            parameters = Sectiondata;
            */

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
            double UtilY8 = 0;
            double UtilZ8 = 0;
            double Util9;

            string Rep0;
            string Rep1;
            string RepY2;
            string RepZ2;
            string RepY3;
            string RepZ3;
            string Rep4;
            string RepY5;
            string RepZ5;
            string RepY6;
            string RepZ6;
            string RepY7;
            string RepZ7;
            string RepY8;
            string RepZ8;
            string Rep9;

            List<double[]> AllUtilsY = new List<double[]>();
            List<double[]> AllUtilsZ = new List<double[]>();
            List<string[]> AllRepsY = new List<string[]>();
            List<string[]> AllRepsZ = new List<string[]>();

            List<string[]> AllULSReports = new List<string[]>();

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

                ///0 "Tension along the grain acc. to EC5 6.1.2",
                if (sigN < 0) Util0 = 0;
                else Util0 = Math.Abs(sigN) / ft0d;

                ///1 "Compression along the grain acc. to EC5 6.1.4"
                if (sigN > 0) Util1 = 0;
                else Util1 = Math.Abs(sigN) / fc0d;

                ///2 "Bending acc. to EC5 6.1.6"
                UtilY2 = Math.Abs(sigMy / fmd) + Km * Math.Abs(sigMz / fmd);
                UtilZ2 = Km * Math.Abs(sigMy / fmd) + Math.Abs(sigMz / fmd);

                ///3 "Shear acc. to EC5 6.1.7"
                UtilY3 = Math.Abs(Sigvy) / fvd;
                UtilZ3 = Math.Abs(Sigvz) / fvd;

                ///4 "Torsion acc. to EC5 6.1.8"
                Util4 = Math.Abs(SigMt) / (kshape * fvd);

                ///5 "Combined Bending and Axial Tension acc. to EC5 6.2.3"
                if (sigN < 0) UtilY5 = UtilZ5 = 0;
                else
                {
                    UtilY5 = sigN / ft0d + Math.Abs(sigMy / fmd) + Km * Math.Abs(sigMz / fmd);
                    UtilZ5 = sigN / ft0d + Km * Math.Abs(sigMy / fmd) + Math.Abs(sigMz / fmd);
                }

                /// 6 "Combined Bending and Axial Compression acc. to EC5 6.2.4"
                if (sigN > 0) UtilY6 = UtilZ6 = 0;
                else
                {
                    UtilY6 = Math.Pow((Math.Abs(sigN) / fc0d), 2) + Math.Abs(sigMy / fmd) + Km * Math.Abs(sigMz / fmd);
                    UtilZ6 = Math.Pow((Math.Abs(sigN) / fc0d), 2) + Km * Math.Abs(sigMy / fmd) + Math.Abs(sigMz / fmd);
                }

                ///7 "Compressed member subjected to either Compression or combined Compression and Bending acc. to EC5 6.3.2 (buckling about both axes considered)"
                if (sigN > 0) UtilY7 = UtilZ7 = 0;
                else
                {
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
                }

                ///8 "Flexural member subjected to either Bending or combined Bending and Compression acc. to EC5 6.3.3 (lateral torsional buckling considered)"
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

                /*
                if(Util1 > Math.Max(UtilY2, UtilZ2))
                /// If compression is more critical than bending, treat it as a column
                {
                    ///7 "Compressed member subjected to either Compression or combined Compression and Bending acc. to EC5 6.3.2 (buckling about both axes considered)"
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
                    ///8 "Flexural member subjected to either Bending or combined Bending and Compression acc. to EC5 6.3.3 (lateral torsional buckling considered)"
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
                */

                ///9 "Combined Shear and Torsion - Not specified in EC5"
                Util9 = Math.Max(UtilY3 + Util4, UtilZ3 + Util4);
                // Less conservative approach: 
                // Util9 = Math.Max(Math.Pow(UtilY3,2) + Util4, Math.Pow(UtilZ3,2) + Util4);

                List<double> UtilsY = new List<double>() { Util0, Util1, UtilY2, UtilY3, Util4, UtilY5, UtilY6, UtilY7, UtilY8, Util9 };
                List<double> UtilsZ = new List<double>() { Util0, Util1, UtilZ2, UtilZ3, Util4, UtilZ5, UtilZ6, UtilZ7, UtilZ8, Util9 };
                List<string> RepsY = new List<string>() { Rep0, Rep1, RepY2, RepY3, Rep4, RepY5, RepY6, RepY7, RepY8, Rep9 };
                List<string> RepsZ = new List<string>() { Rep0, Rep1, RepZ2, RepZ3, Rep4, RepZ5, RepZ6, RepZ7, RepZ8, Rep9 };

                List<string> ULSReports = new List<string>()
                {
                    //0
                    "Tension along the grain acc. to EC5 6.1.2 | " +
                    "N = " + f.N.ToString() + "; sigN = " + sigN.ToString() + "; Kmod = " + Kmod.ToString() + "; Ym = " + Ym.ToString() + "; ft0d = " + ft0d.ToString() +"; R0 = " + Util0.ToString(),
                    //1
                    "Compression along the grain acc. to EC5 6.1.4 | " +
                    "N = " + f.N.ToString() + "; sigN = " + sigN.ToString() + "; Kmod = " + Kmod.ToString() + "; Ym = " + Ym.ToString() + "; fc0d = " + fc0d.ToString() + "; R1 = " + Util1.ToString(),
                    //2
                    "Bending acc. to EC5 6.1.6 | " + 
                    "Wy = " + Wy.ToString() + "; Wz = " + Wz.ToString() + "; fmk = " + fmk.ToString() + "; Myd = " + Myd.ToString() + "; Mzd = " + Mzd.ToString() + "; sigMy = " + sigMy.ToString() + 
                    "; Kmod = " + Kmod.ToString() + "; Ym = " + Ym.ToString() + "; fmd = " + fmd.ToString() + "; Km = " + Km.ToString() + "; R2y = " + UtilY2.ToString() + "&" + "R2z = " + UtilZ2.ToString(),
                    //3
                     "Shear acc. to EC5 6.1.7 | " +
                     "kcrit = " + kcrit.ToString() + "; Vy = " + Vy.ToString() + "; Vz = " + Vz.ToString() + "; A = " + CS.A.ToString() + "; sigVy = " + Sigvy.ToString() + "; sigVz = " + Sigvz.ToString() + 
                     "; R3y = " + UtilY3.ToString() + "&" +  "R3z = " + UtilZ3.ToString(),
                    //4
                    "Torsion acc. to EC5 6.1.8 | " +
                    "; It = " + CS.It.ToString() + "; Kshape = " + kshape.ToString() + "; Mt = " + Mt.ToString() + "; SigMt = " + SigMt.ToString() + "; Kmod = " + Kmod.ToString() + "; Ym = " + Ym.ToString() +
                    "; fvd = " + fvd.ToString() + "; R4 = " + Util4.ToString(),
                    //5
                    "Combined Bending and Axial Tension acc. to EC5 6.2.3 | " +
                    "A = " + A.ToString() + "; Wy = " + Wy.ToString() + "; Wz = " + Wz.ToString() + "; Nd = " + Nd.ToString() + "; Myd = " + Myd.ToString() + "; Mzd = " + Mzd.ToString() + "; sigN = " + sigN.ToString() +
                    "; sigMy = " + sigMy.ToString() +  "; sigMz = " + sigMz.ToString() + "; Kmod = " + Kmod.ToString() + "; Ym = " + Ym.ToString() + "; ft0d = " + ft0d.ToString() + "; fmd = " + fmd.ToString() +  
                    "; Km = " + Km.ToString() + "; R5y = " + UtilY5.ToString() + "&" + "R5z = " + UtilZ5.ToString(),
                    //6
                    "Combined Bending and Axial Compression acc. to EC5 6.2.4 | " +
                    "A = " + A.ToString() + "; Wy = " + Wy.ToString() + "; Wz = " + Wz.ToString() + "; Nd = " + Nd.ToString() + "; Myd = " + Myd.ToString() + "; Mzd = " + Mzd.ToString() + "; sigN = " + sigN.ToString() +
                    "; sigMy = " + sigMy.ToString() +  "; sigMz = " + sigMz.ToString() + "; Kmod = " + Kmod.ToString() + "; Ym = " + Ym.ToString() + "; fc0d = " + fc0d.ToString() + "; fmd = " + fmd.ToString() +
                    "; Km = " + Km.ToString() + "; R6y = " + UtilY6.ToString() + "&" + "R6z = " + UtilZ6.ToString(),
                    //7
                    "Compressed member subjected to either Compression or combined Compression and Bending acc. to EC5 6.3.2 (buckling about both axes considered) | " +
                    "A = " + A.ToString() + "; Wy = " + Wy.ToString() + "; Wz = " + Wz.ToString() + "; Nd = " + Nd.ToString() + "; Myd = " + Myd.ToString() + "; Mzd = " + Mzd.ToString() + "; sigN = " + sigN.ToString() +
                    "; sigMy = " + sigMy.ToString() +  "; sigMz = " + sigMz.ToString() + "; ly = " + ly.ToString() + "; lz = " + lz.ToString() + "; ry = " + ry.ToString() + "; rz = " + rz.ToString() +
                    "; lampi = " + lampi.ToString() + "; lamy = " + lamy.ToString() + "; lamz = " + lamz.ToString() + "; lamyrel = " + lamyrel.ToString() + "; lamzrel = " + lamzrel.ToString() + "; ky = " + ky.ToString() +
                    "; kz = " + kz.ToString() + "; kyc = " + kyc.ToString() + "; kzc = " + kzc.ToString() + "; Kmod = " + Kmod.ToString() + "; Ym = " + Ym.ToString() + "; fc0d = " + fc0d.ToString() +
                    "; fmd = " + fmd.ToString() +  "; Km = " + Km.ToString() + "; R7y = " + UtilY7.ToString() + "&" + "R7z = " + UtilZ7.ToString(),
                    //8
                    "Flexural member subjected to either Bending or combined Bending and Compression acc. to EC5 6.3.3 (lateral torsional buckling considered) | " +
                    "A = " + A.ToString() + "; Wy = " + Wy.ToString() + "; Wz = " + Wz.ToString() + "; Nd = " + Nd.ToString() + "; Myd = " + Myd.ToString() + "; Mzd = " + Mzd.ToString() + "; sigN = " + sigN.ToString() +
                    "; sigMy = " + sigMy.ToString() +  "; sigMz = " + sigMz.ToString() + "; ly = " + ly.ToString() + "; lz = " + lz.ToString() + "; ry = " + ry.ToString() + "; rz = " + rz.ToString() +
                    "; lampi = " + lampi.ToString() + "; lamy = " + lamy.ToString() + "; lamz = " + lamz.ToString() + "; lamyrel = " + lamyrel.ToString() + "; lamzrel = " + lamzrel.ToString() + "; ky = " + ky.ToString() +
                    "; kz = " + kz.ToString() + "; kyc = " + kyc.ToString() + "; kzc = " + kzc.ToString() + "; kflam = " + kflam.ToString() + "; lefy = " + lefy.ToString() + "; lefz = " + lefz.ToString() +  
                    "; sigMcrity = " + sigMcrity.ToString() + "; sigMcritz = " + sigMcritz.ToString() + "; lammy = " + lammy.ToString() + "; lammz = " + lammz.ToString() + "; kcrity = " + kcrity.ToString() + 
                    "; kcritz = " + kcritz.ToString() + "; Kmod = " + Kmod.ToString() + "; Ym = " + Ym.ToString() + "; fc0d = " + fc0d.ToString() + "; fmd = " + fmd.ToString() +  "; Km = " + Km.ToString() +
                    "R8y = " + UtilY8.ToString() + "&" + "R8z = " + UtilZ8.ToString(),
                    //9
                    "Combined Torsion and Shear - Not speciefied in EC5 (Maximum Shear Utilization Ratio + Torsion Utilization Ratio) | " +
                    "R9 = R3max + R4 = " + Util9.ToString(),
                };

                AllUtilsY.Add(UtilsY.ToArray());
                AllUtilsZ.Add(UtilsZ.ToArray());
                AllRepsY.Add(RepsY.ToArray());
                AllRepsZ.Add(RepsZ.ToArray());

                AllULSReports.Add(ULSReports.ToArray());

            }


            TimberFrameULSResult Result = new TimberFrameULSResult(AllULSReports, AllUtilsY, AllUtilsZ);

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
