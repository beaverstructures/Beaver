using BeaverCore.Actions;
using BeaverCore.CrossSection;
using BeaverCore.Misc;
using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeaverCore.Geometry;

namespace BeaverCore.Frame

{
    public enum SpanType
    {
        Span,
        CantileverSpan
    }

    /// <summary>
    /// a TimberFrame element for calculating stresses and displacements on a given element
    /// </summary>
    public class TimberFrame
    {
        /// <summary>
        /// Mapping between TimberFramePoints and it's
        /// relative positions [0,1].
        /// </summary>
        public Dictionary<double, TimberFramePoint> TimberPointsMap;

        /// <summary>
        /// Geometric representation of the member axis.
        /// </summary>
        public Line FrameAxis;

        public TimberFrame(Dictionary<double, TimberFramePoint> timberpoints)
        {
            TimberPointsMap = new Dictionary<double, TimberFramePoint>(timberpoints);
        }

        public TimberFrame(Dictionary<double, TimberFramePoint> timberpoints, Line line)
        {
            TimberPointsMap = new Dictionary<double, TimberFramePoint>(timberpoints);
            FrameAxis = line;
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
        public SpanType span_type;
        public double ly;
        public double lz;
        public double kflam;
        public double lspan;
        public double[] inst_deflection_limit;
        public double[] netfin_deflection_limit;
        public double[] fin_deflection_limit;
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
            CS.Mat.Setkdef(sc);
            SLSComb = new SLSCombinations(disp, sc, CS.Mat);
            this.ly = ly;
            this.lz = lz;
            this.kflam = kflam;
            this.lspan = lspan;
            span_type = SpanType.Span;
        }

        public TimberFramePoint(List<Force> forces, List<Displacement> disp, CroSec cs, int sc, double ly, double lz, double lspan, double kflam, SpanType span_type)
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
            this.span_type = span_type;
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
                    "EC5 Section 6.3.2 Columns subjected to either compression or combined compression and bending",
                    //7
                    "EC5 Section 6.3.3 Beams subjected to either bending or combined bending and compression",
                };



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
            string Sectiondata = string.Format(
                "kflam = {0}, kcrit ={1}, kshape = {2}, λy ={3}, λz ={4},λrely ={5}, λrelz ={6}, ky ={7}, kz ={8}, kcy={9}, kcz={10}, σMcrity= {11}, σMcritz= {12}, λmy ={13}, λmz ={14}",
                    Math.Round(kflam, 3), Math.Round(kcrit, 3), Math.Round(kcrit, 3), Math.Round(lamy, 3), Math.Round(lamz, 3), Math.Round(lamyrel, 3), Math.Round(lamzrel, 3), Math.Round(ky, 3), Math.Round(kz, 3),
                    Math.Round(kyc, 3), Math.Round(kzc, 3), Math.Round(sigMcrity, 3), Math.Round(sigMcritz, 3), Math.Round(lammy, 3), Math.Round(lammz, 3));
            parameters = Sectiondata;

            //Define Utilization
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
            List<double[]> AllUtilsY = new List<double[]>();
            List<double[]> AllUtilsZ = new List<double[]>();
            foreach (Force f in ULSComb.DesignForces)
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
                else
                {
                    UtilY7 = 0;
                    UtilZ7 = 0;
                }

                List<double> UtilsY = new List<double>() { Util0, Util1, UtilY2, UtilY3, Util4, UtilY5, UtilY6, UtilY7 };
                List<double> UtilsZ = new List<double>() { Util0, Util1, UtilZ2, UtilZ3, Util4, UtilZ5, UtilZ6, UtilZ7 };
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

        private double GetDisplacementLimit(double[] displacement_limit)
        {
            double result = 0;
            if (span_type is SpanType.Span)
            {
                result = displacement_limit[0];
            }
            else if (span_type is SpanType.CantileverSpan)
            {
                result = displacement_limit[1];
            }
            return result;
        }


        public List<double> InstDisplacementUtil()
        {
            double deflection_limit = GetDisplacementLimit(inst_deflection_limit);
            List<double> disps_ratio = new List<double>();
            foreach (var disp in SLSComb.CharacteristicDisplacements)
            {
                disps_ratio.Add((disp.Absolute())/ deflection_limit);
            }
            return disps_ratio;
        }

        public List<double> NetFinDisplacementUtil()
        {
            double deflection_limit = GetDisplacementLimit(inst_deflection_limit);
            List<double> disps_ratio = new List<double>();
            foreach (var disp in SLSComb.CreepDisplacements)
            {
                disps_ratio.Add((disp.Absolute() - precamber) / deflection_limit);
            }
            return disps_ratio;
        }

        public List<double> FinDisplacementUtil()
        {
            double deflection_limit = GetDisplacementLimit(inst_deflection_limit);
            List<double> disps_ratio = new List<double>();
            foreach (var disp in SLSComb.CreepDisplacements)
            {
                disps_ratio.Add((disp.Absolute()) / deflection_limit);
            }
            return disps_ratio;
        }

    }

    public class TimberFrameULSResult
    {
        string[] Info;
        List<double[]> UtilsY;
        List<double[]> UtilsZ;
        string SectionData;

        public TimberFrameULSResult() { }

        public TimberFrameULSResult(string[] info, List<double[]> utilsY, List<double[]> utilsZ, string sectiondata)
        {
            Info = info;
            UtilsY = utilsY;
            UtilsZ = utilsZ;
            SectionData = sectiondata;
        }
    }

    public class TimberFrameSLSResult
    {
        string[] Info;
        List<double> InstUtils;
        List<double> NetFinUtils;
        List<double> FinUtils;

        public TimberFrameSLSResult() { }

        public TimberFrameSLSResult(string[] info, List<double> instUtils, List<double> netFinUtils, List<double> finUtils)
        {
            Info = info;
            InstUtils = instUtils;
            NetFinUtils = netFinUtils;
            FinUtils = finUtils;
        }
    }
}
