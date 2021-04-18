using BeaverCore.Actions;
using BeaverCore.CrossSection;
using BeaverCore.Misc;
using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaverCore.Frame

{
    public class TimberFrame
    {
        /// <summary>
        /// a TimberFrame element for calculating stresses and displacements on a given element
        /// </summary>
        public Dictionary<double,TimberFramePoint> TimberPointsMap;

        public TimberFrame(Dictionary<double, TimberFramePoint> timberpoints)
        {
            TimberPointsMap = new Dictionary<double, TimberFramePoint>(timberpoints);
        }
    }

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
        public double ly;
        public double lz;
        public double kflam;
        public double lspan;
        public double deflection_limit;
        public double precamber;
        public string id;
        public string guid;
        public string parameters;
        public int sc;

        public TimberFramePoint() { }

        public TimberFramePoint(List<Force> forces, List<Displacement> disp, CroSec cs, int sc, double ly, double lz, double lspan, double kflam)
        {
            Forces = forces;
            Disp = disp;
            this.sc = sc;
            ULSComb = new ULSCombinations(forces, sc);
            CS = cs;
            SLSComb = new SLSCombinations(disp, sc, cs.Mat);
            this.ly = ly;
            this.lz = lz;
            this.kflam = kflam;
            this.lspan = lspan;
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

        // Section Analisys
        public void Utilization()
        {
            List<string> info = new List<string>(new string[] {
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
                    "EC5 Section 6.3.2 Columns subjected to either compression or combined compression and bending",
                    //7
                    "EC5 Section 6.3.3 Beams subjected to either bending or combined bending and compression",
                });

            double Util0 = 0;
            double Util1 = 0;
            double UtilY2 = 0;
            double UtilZ2 = 0;
            double UtilY3 = 0;
            double UtilZ3 = 0;
            double Util4 = 0;
            double UtilY5 = 0;
            double UtilZ5 = 0;
            double UtilY6 = 0;
            double UtilZ6 = 0;
            double UtilY7 = 0;
            double UtilZ7 = 0;

            //Basic Geometry and Material Values
            double A = CS.A;
            double Wy = CS.Wy;
            double Wz = CS.Wz;
            double ry = CS.ry;
            double rz = CS.rz;

            double Km = 1;
            double Ym = CS.Mat.Ym;
            double fc0k = CS.Mat.fc0k;
            double ft0k = CS.Mat.ft0k;
            double fmk = CS.Mat.fmk;
            double Fvk = CS.Mat.fvk;
            double E05 = CS.Mat.E05;
            double G05 = CS.Mat.G05;
            double Bc = 0.2;

            //Define EC5 Section 6.1.7 Shear Coefficients
            double kcrit = 0.67;

            //Define EC5 Section 6.1.8 Torsion Coefficients
            double kshape = 2;
            CroSec_Rect cs = (CroSec_Rect)CS;
            if (CS is CroSec_Rect) { kshape = Math.Min(1 + 0.15 * (cs.h / cs.b), 2); };
            if (CS is CroSec_Circ) { kshape = 1.2; };

            //Define EC5 Section 6.2.3 & 6.3.2 Coefficients
            double lefy = ly * kflam * 100;
            double lefz = lz * kflam * 100;
            double sigMcrity = CS.GetsigMcrit(lefy, E05, G05);
            double sigMcritz = CS.GetsigMcrit(lefz, E05, G05);
            double lammy = Math.Sqrt(fmk / sigMcrity);
            double lammz = Math.Sqrt(fmk / sigMcritz);
            double kcrity = Getkcrit(lammy);
            double kcritz = Getkcrit(lammz);

            //Define EC5 Section 6.3.2 Coefficients
            double lamy = (100 * ly) / ry;
            double lamz = (100 * lz) / rz;
            double lampi = Math.Sqrt(fc0k / E05) / Math.PI;
            double lamyrel = lamy * lampi;
            double lamzrel = lamz * lampi;
            double ky = 0.5 * (1 + Bc * (lamyrel - 0.3) + Math.Pow(lamyrel, 2));
            double kz = 0.5 * (1 + Bc * (lamzrel - 0.3) + Math.Pow(lamzrel, 2));
            double kyc = 1 / (ky + Math.Sqrt(Math.Pow(ky, 2) - Math.Pow(lamyrel, 2)));
            double kzc = 1 / (kz + Math.Sqrt(Math.Pow(kz, 2) - Math.Pow(lamzrel, 2)));

            //Define Basic Coefficients Output
            string SectionULSdata = string.Format(
                "kfalm = {0}, kcrit ={1}, kshape = {2}, λy ={3}, λz ={4},λrely ={5}, λrelz ={6}, ky ={7}, kz ={8}, kcy={9}, kcz={10}, σMcrity= {11}, σMcritz= {12}, λmy ={13}, λmz ={14}",
                    Math.Round(kflam, 3), Math.Round(kcrit, 3), Math.Round(kcrit, 3), Math.Round(lamy, 3), Math.Round(lamz, 3), Math.Round(lamyrel, 3), Math.Round(lamzrel, 3), Math.Round(ky, 3), Math.Round(kz, 3),
                    Math.Round(kyc, 3), Math.Round(kzc, 3), Math.Round(sigMcrity, 3), Math.Round(sigMcritz, 3), Math.Round(lammy, 3), Math.Round(lammz, 3));
            parameters = SectionULSdata;

            //Define Utilization
            foreach (Force f in ULSComb.Sd)
            {
                //Actions
                double Nd = f.N;
                double Vy = f.Vy;
                double Vz = f.Vz;
                double Myd = f.My;
                double Mzd = f.Mz;
                double Mt = f.Mt;
                double sigN = Nd / A;
                double Sigvy = (3 / 2) * (Vy / (kcrit * CS.A));
                double Sigvz = (3 / 2) * (Vz / (kcrit * CS.A));
                double sigMy = 100 * Math.Abs(Myd) / Wy;
                double sigMz = 100 * Math.Abs(Mzd) / Wz;
                double SigMt = Math.Abs(Mt) / CS.It;

                //Resistances
                double Kmod = Utils.KMOD(ULSComb.SC, f.duration);
                double fc0d = Kmod * fc0k / Ym;
                double ft0d = Kmod * ft0k / Ym;
                double fvd = Kmod * Fvk / Ym;
                double fmd = Kmod * fmk / Ym;

                //0 EC5 Section 6.1.2 Tension parallel to the grain
                Util0 = sigN / fc0d;

                //1 EC5 Section 6.1.4 Compression parallel to the grain
                Util1 = sigN / fc0d;

                //2 EC5 Section 6.1.6 Biaxial Bending
                UtilY2 = sigN / fc0d + (sigMy / fmd) + Km * (sigMz / fmd);
                UtilZ2 = sigN / fc0d + Km * (sigMy / fmd) + (sigMz / fmd);

                //3 EC5 Section 6.1.7 Shear
                UtilY3 = Sigvy / fvd;
                UtilZ3 = Sigvz / fvd;

                //4 EC5 Section 6.1.8 Torsion
                Util4 = SigMt / (kshape / fvd);

                //5 EC5 Section 6.2.3 Combined Tension and Bending
                if (Math.Max(lammy, lammz) >= 0.75 && Math.Max(lammy, lammz) < 1.4)
                {
                    UtilY5 = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / ft0d) + Km * (sigMz / fmd);
                    UtilZ5 = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / ft0d) + Km * (sigMy / fmd);
                }
                else if (Math.Max(lammy, lammz) >= 1.4)
                {
                    UtilY5 = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / (ft0d)) + Km * (sigMz / fmd);
                    UtilZ5 = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / (ft0d)) + Km * (sigMy / fmd);
                }
                else
                {
                    UtilY5 = (sigN / ft0d) + (sigMy / fmd) + Km * (sigMz / fmd);
                    UtilZ5 = (sigN / ft0d) + Km * (sigMy / fmd) + (sigMz / fmd);
                }

                //6 EC5 Section 6.3.2 Columns subjected to either compression or combined compression and bending
                if (lamyrel <= 0.3 && lamzrel <= 0.3)
                {
                    UtilY6 = Math.Pow(sigN / fc0d, 2) + (sigMy / fmd) + Km * (sigMz / fmd);
                    UtilZ6 = Math.Pow(sigN / fc0d, 2) + Km * (sigMy / fmd) + (sigMz / fmd);
                }
                else
                {
                    UtilY6 = (sigN / (kyc * fc0d)) + (sigMy / fmd) + Km * (sigMz / fmd);
                    UtilZ6 = (sigN / (kzc * fc0d)) + Km * (sigMy / fmd) + (sigMz / fmd);
                }

                //7 EC5 Section 6.3.3 Beams subjected to either bending or combined bending and compression
                if (Math.Max(lammy, lammz) >= 0.75 && Math.Max(lammy, lammz) < 1.4)
                {
                    UtilY7 = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / (kzc * fc0d)) + Km * (sigMz / fmd);
                    UtilZ7 = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / (kyc * fc0d)) + Km * (sigMy / fmd); ;
                }
                if (Math.Max(lammy, lammz) >= 1.4)
                {
                    UtilY7 = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / (kzc * fc0d)) + Km * (sigMz / fmd);
                    UtilZ7 = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / (kyc * fc0d)) + Km * (sigMy / fmd);
                }

                List<double> UtilsY = new List<double>() { Util0, Util1, UtilY2, UtilY3, Util4, UtilY5, UtilY6, UtilY7 };
                List<double> UtilsZ = new List<double>() { Util0, Util1, UtilZ2, UtilZ3, Util4, UtilZ5, UtilZ6, UtilZ7 };

                //$$$OUTPUT TA ZUADO

            }



        }




        public List<double> CharacteristicDisplacementUtil()
        {
            /// Calculates the ratio between calculated dispacements and allowed displacements for the characteristic combination
            List<double> disps_ratio = new List<double>();
            foreach (var disp in SLSComb.CharacteristicDisplacements)
            {
                disps_ratio.Add(disp.Absolute() + precamber / deflection_limit);
            }
            return disps_ratio;
        }

        public List<double> LongtermDisplacementUtil()
        {
            /// Calculates the ratio between calculated dispacements and allowed displacements for the quasi-permanent combination considering creep deformations
            List<double> disps_ratio = new List<double>();
            foreach (var disp in SLSComb.CreepDisplacements)
            {
                disps_ratio.Add(disp.Absolute() + precamber / deflection_limit);
            }
            return disps_ratio;
        }        
    }
}
