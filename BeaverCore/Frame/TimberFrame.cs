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
        //$$$Add docstring urgent!!
        public Dictionary<double,TimberFramePoint> TimberPointsMap;

        public TimberFrame(Dictionary<double, TimberFramePoint> timberpoints)
        {
            TimberPointsMap = new Dictionary<double, TimberFramePoint>(timberpoints);
        }
    }

    public class TimberFramePoint
    {
        public List<Force> Forces;
        public List<Displacement> Disp;
        public ULSCombinations IForces;
        public SLSCombinations SLSComb;
        public CroSec CS;
        public double ly;
        public double lz;
        public double lspan;
        public double deflection_limit;
        public double precamber;
        public string id;
        public string guid;
        public string parameters;
        public int sc;

        public TimberFramePoint() { }
        public TimberFramePoint(ULSCombinations act, CroSec cs,double ly, double lz, double lspan)
        {
            IForces = act;
            CS = cs;
            this.ly = ly;
            this.lz = lz;
            this.lspan = lspan;
        }

        public TimberFramePoint(List<Force> forces, List<Displacement> disp, CroSec cs, int sc, double ly, double lz, double lspan)
        {
            Forces = forces;
            Disp = disp;
            this.sc = sc;
            IForces = new ULSCombinations(forces, sc);
            CS = cs;
            this.ly = ly;
            this.lz = lz;
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
        public List<double[]> BendingNormalUtil()
        {
            double Km = 1;
            double Ym = CS.Mat.Ym;
            double fc0k = CS.Mat.fc0k;
            double ft0k = CS.Mat.ft0k;
            double fmk = CS.Mat.fmk;
            double E05 = CS.Mat.E005;
            double kflam = 0.9;
            double Bc = 0.2;


            //Definição de valores geométricos
            double A = CS.A;
            double Iy = CS.Iy;
            double Iz = CS.Iz;
            double Wy = CS.Wy;
            double Wz = CS.Wz;
            double ry = CS.ry;
            double rz = CS.rz;
            double lamy = (100 * ly) / ry;
            double lamz = (100 * lz) / rz;
            double lefy = ly * kflam * 100;
            double lefz = lz * kflam * 100;

            //Definição de valores do material 
            double lampi = Math.Sqrt(fc0k / E05) / Math.PI;

            double lamyrel = lamy * lampi;
            double lamzrel = lamz * lampi;

            double G05 = E05 / 16;
            double UtilY = 0;
            double UtilZ = 0;
            string info = "";

            //Definição dos valores de cálculo necessários para verificação em pilares ou vigas 
            double sigMcrity = CS.GetsigMcrit(lefy, E05);
            double sigMcritz = CS.GetsigMcrit(lefz, E05);
            double lammy = Math.Sqrt(fmk / sigMcrity);
            double lammz = Math.Sqrt(fmk / sigMcritz);
            double ky = 0.5 * (1 + Bc * (lamyrel - 0.3) + Math.Pow(lamyrel, 2));
            double kz = 0.5 * (1 + Bc * (lamzrel - 0.3) + Math.Pow(lamzrel, 2));
            double kyc = 1 / (ky + Math.Sqrt(Math.Pow(ky, 2) - Math.Pow(lamyrel, 2)));
            double kzc = 1 / (kz + Math.Sqrt(Math.Pow(kz, 2) - Math.Pow(lamzrel, 2)));
            string data = string.Format("λy ={0}, λz ={1},λrely ={2}, λrelz ={3}, ky ={4}, kz ={5}, kcy={6}, kcz={7}, σMcrity= {8}, σMcritz= {9}, λmy ={10}, λmz ={11}",
                    Math.Round(lamy, 3), Math.Round(lamz, 3), Math.Round(lamyrel, 3), Math.Round(lamzrel, 3), Math.Round(ky, 3), Math.Round(kz, 3),
                    Math.Round(kyc, 3), Math.Round(kzc, 3), Math.Round(sigMcrity, 3), Math.Round(sigMcritz, 3), Math.Round(lammy, 3), Math.Round(lammz, 3));
            parameters = data;
            List<double[]> result = new List<double[]>();

            foreach (Force f in IForces.Sd)
            {
                string loaddata = "";
                double Nd = f.InternalForces["N"];
                double Myd = f.InternalForces["My"];
                double Mzd = f.InternalForces["Mz"];
                double Kmod = Utils.KMOD(IForces.SC, f.duration);
                double fc0d = Kmod * fc0k / Ym;
                double ft0d = Kmod * ft0k / Ym;
                double fmd = Kmod * fmk / Ym;
                double sigN = Nd / A;
                double sigMy = 100 * Math.Abs(Myd) / Wy;
                double sigMz = 100 * Math.Abs(Mzd) / Wz;


                if (Nd > 0)
                {
                    if (Myd == 0 && Mzd == 0)
                    {
                        loaddata = "Pure Tension";
                    }
                    else
                    {
                        loaddata = "Tension + Bending";
                    }
                }
                else if (Nd < 0)
                {
                    if (Myd == 0 && Mzd == 0)
                    {
                        loaddata = "Pure Compression";
                    }
                    else
                    {
                        loaddata = "Compression + Bending";
                    }
                }
                else if (Nd == 0 && (Myd != 0 || Mzd != 0))
                {
                    loaddata = "Pure Bending";
                }
                

                if (Nd < 0)
                {
                    Nd = Math.Abs(Nd);
                    sigN = Math.Abs(sigN);
                    //Verificação de comportamento de Pilares
                    if (Math.Max(lammy, lammz) < 0.75) //checar se é isso mesmo, ou devo considerar apenas lammy
                    {
                        if (lamyrel <= 0.3 && lamzrel <= 0.3)
                        {
                            UtilY = Math.Pow(sigN / fc0d, 2) + (sigMy / fmd) + Km * (sigMz / fmd);
                            UtilZ = Math.Pow(sigN / fc0d, 2) + Km * (sigMy / fmd) + (sigMz / fmd);
                            info = loaddata + ", No Buckling effect:" + data;
                        }
                        else
                        {
                            UtilY = (sigN / (kyc * fc0d)) + (sigMy / fmd) + Km * (sigMz / fmd);
                            UtilZ = (sigN / (kzc * fc0d)) + Km * (sigMy / fmd) + (sigMz / fmd);

                        }

                        info = loaddata + ", Acts as a Column (λm<0.75): " + data;
                    }
                    //Verificação de comportamento de Vigas
                    else
                    {

                        if (Math.Max(lammy, lammz) >= 0.75 && Math.Max(lammy, lammz) < 1.4)
                        {
                            double kcrity = Getkcrit(lammy);
                            double kcritz = Getkcrit(lammz);
                            UtilY = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / (kzc * fc0d)) + Km * (sigMz / fmd);
                            UtilZ = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / (kyc * fc0d)) + Km * (sigMy / fmd); ;
                            info = loaddata + ", Acts as a Beam (0.75 <= λm < 1.4): " + data;

                        }
                        if (Math.Max(lammy, lammz) >= 1.4)
                        {
                            double kcrity = Getkcrit(lammy);
                            double kcritz = Getkcrit(lammz);
                            UtilY = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / (kzc * fc0d)) + Km * (sigMz / fmd);
                            UtilZ = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / (kyc * fc0d)) + Km * (sigMy / fmd);
                            info = loaddata + ", Acts as a Beam (λm >= 1.4): " + data;
                        }



                    }



                }
                else if (Nd >= 0)
                {
                    if (Math.Max(lammy, lammz) >= 0.75 && Math.Max(lammy, lammz) < 1.4)
                    {
                        double kcrity = Getkcrit(lammy);
                        double kcritz = Getkcrit(lammz);
                        UtilY = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / ft0d) + Km * (sigMz / fmd);
                        UtilY = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / ft0d) + Km * (sigMy / fmd);
                        info = loaddata + ", Acts as a Beam (0.75 <= λm < 1.4): " + data;
                    }
                    else if (Math.Max(lammy, lammz) >= 1.4)
                    {
                        double kcrity = Getkcrit(lammy);
                        double kcritz = Getkcrit(lammz);
                        UtilY = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / (ft0d)) + Km * (sigMz / fmd);
                        UtilY = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / (ft0d)) + Km * (sigMy / fmd);
                        info = loaddata + ", Acts as a Beam (λm >= 1.4): " + data;
                    }
                    else
                    {
                        UtilY = (sigN / ft0d) + (sigMy / fmd) + Km * (sigMz / fmd);
                        UtilZ = (sigN / ft0d) + Km * (sigMy / fmd) + (sigMz / fmd);
                        info = loaddata + ", No Torsional Buckling effects, λmy=" + Math.Round(lammy, 3) + ", λmyz=" + Math.Round(lammz, 3);
                    }
                }
                double[] iresult = new double[] { UtilY, UtilZ };
                result.Add(iresult);
            }
            return result;

            
        }
        public List<double[]> ShearUtil()
        {
            List<double[]> result = new List<double[]>();


            double Ym = CS.Mat.Ym;
            double Fvk = CS.Mat.fvk;


            double kcrit = 0.67;

            foreach (Force f in IForces.Sd)
            {
                double Kmod = Utils.KMOD(IForces.SC, f.duration);
                double Vy = f.InternalForces["Vy"];
                double Vz = f.InternalForces["Vz"];

                double Sigvy = (3 / 2) * (Vy / (kcrit * CS.A));
                double Sigvz = (3 / 2) * (Vz / (kcrit * CS.A));

                double fvd = Kmod * Fvk / Ym;
                double Utily = Sigvy / fvd;
                double Utilz = Sigvz / fvd;
                result.Add(new double[] { Utily, Utilz });
            }
            return result;
        }
        public List<double> TorsionUtil()
        {
            List<double> result = new List<double>();

            double kshape = 1;
            if (CS is CroSec_Rect)
            {
                CroSec_Rect cs = (CroSec_Rect)CS;
                kshape = Math.Min(1 + 0.15 * (cs.h / cs.b), 2);
            }
            double Ym = CS.Mat.Ym;
            double Fvk = CS.Mat.fvk;
            foreach (Force f in IForces.Sd) {
                double Mt = 0;
                double Kmod = Utils.KMOD(IForces.SC,f.duration);
                double Sigt = Mt / CS.It;
                double fvd = Kmod * Fvk / Ym;
                double Util = Sigt / (kshape / fvd);
                result.Add(Util);
            }

            return result;
        }

        public List<double> CharacteristicDisplacementUtil()
        {
            /// Calculates the ratio between calculated dispacements and allowed displacements for the characteristic combination
            List<double> disps_ratio = new List<double>();
            foreach (var disp in SLSComb.CharacteristicDisplacements)
            {
                disps_ratio.Add(disp.Absolute() / deflection_limit);
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
