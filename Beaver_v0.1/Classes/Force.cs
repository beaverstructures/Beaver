using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Beaver_v0._1.Classes
{
    using IF = Dictionary<string, double>;

    public class Force
    {
        public IF InternalForces = new IF() {
        {"N",0},{"Vy",0},{"Vz",0},{"Mt",0},{"My",0},{"Mz",0}
        };
        public string type;

        public Force() { }

        public Force(double n, double my, double mz, double vy, double vz, double mt, string type)
        {
            InternalForces["N"] = n;
            InternalForces["Vy"] = vy;
            InternalForces["Vz"] = vz;
            InternalForces["Mt"] = mt;
            InternalForces["My"] = my;
            InternalForces["Mz"] = mz;
            this.type = type;

        }

        public static Force operator +(Force f1, Force f2)
        {
            Force result = new Force();
            foreach (KeyValuePair<string, double> f in f1.InternalForces)
            {
                result.InternalForces[f.Key] = f.Value + f2.InternalForces[f.Key];
            }
            result.type = f1.type;
            return result;
        }

        public static Force operator *(double s, Force f1)
        {
            Force result = new Force();
            foreach (KeyValuePair<string, double> f in f1.InternalForces)
            {
                result.InternalForces[f.Key] = s * f.Value;
            }
            result.type = f1.type;
            return result;
        }

        public static Force operator *(Force f1, double s)
        {
            Force result = new Force();
            foreach (KeyValuePair<string, double> f in f1.InternalForces)
            {
                result.InternalForces[f.Key] = s * f.Value;
            }
            result.type = f1.type;
            return result;
        }

        public static bool operator *(Force f1, Force f2)
        {
            bool result = true;
            foreach (string key in f1.InternalForces.Keys)
            {
                if (f1.InternalForces[key] * f2.InternalForces[key] < 0)
                {
                    result = false;
                }
            }
            return result;
        }
    }

    public class Combinations
    {
        public Force[] Sd;
        public double[] kmod = new double[] { };
        public string[] info = new string[] { };
        public int SC;
        public Combinations() { }

        public Combinations(List<Force> Sk, int sc)
        {
            SC = sc;
            DesignAction(Sk);
        }

        public void DesignAction(List<Force> Sk)
        {
            //
            //INITIAL DATA
            //
            List<Force> SGk = Sk.Where(x => x.type.Contains("P")).ToList();
            List<Force> SQk = Sk.Where(x => x.type.Contains("Q")).ToList();
            List<Force> SWk = Sk.Where(x => x.type.Contains("W")).ToList();
            List<Force> Sd = new List<Force>();
            List<double> kmod = new List<double>();
            List<string> info = new List<string>();

            //
            //PERMANENT LOADING
            //

            Force P = new Force();
            foreach (Force act in SGk)
            {
                P += act;
            }
            P.type = "P";
            Sd.Add(P);
            kmod.Add(Utils.KMOD(SC, new TypeInfo(P.type).duration));
            info.Add("1.35G");

            //
            //ACIDENTAL LOADING
            //

            for (int i = 0; i < SQk.Count; i++)
            {
                List<Force> SQa = new List<Force>(SQk);
                SQa.RemoveAt(i);
                Force Qmain = SQk[i];
                Force SumQi = new Force();
                for (int j = 0; j < SQa.Count; j++)
                {
                    TypeInfo t = new TypeInfo(SQa[j].type);
                    SumQi += SQa[j] * t.phi0;
                }
                Force A = 1.35 * P + 1.5 * (Qmain + SumQi);
                A.type = Qmain.type;
                Sd.Add(A);
                kmod.Add(Utils.KMOD(SC, new TypeInfo(A.type).duration));
                info.Add("1.35G + 1.5 " + A.type + " + 1.5Σ(φᵢ₀Qᵢ)");

                foreach (Force w in SWk)
                {
                    Sd.Add(A + 0.6 * w);
                    kmod.Add(Utils.KMOD(SC, new TypeInfo(A.type).duration));
                    info.Add("1.35G + 1.5 " + A.type + " + 1.5(Σ(φᵢ₀Qᵢ)+0.6W)");
                }
            }

            //
            //WIND LOADING
            //

            List<Action> WComb = new List<Action>();
            Force SumQ = new Force();
            foreach (Force Q in SQk)
            {
                TypeInfo t = new TypeInfo(Q.type);
                SumQ += Q * t.phi0;
            }
            foreach (Force W in SWk)
            {
                kmod.Add(Utils.KMOD(SC, (new TypeInfo(W.type)).duration));
                Force Wcomb = new Force();
                if (P*W)
                {
                    if (SumQ * W)
                    {
                        Wcomb = 1.35 * P + 1.5 * (W + SumQ);
                        info.Add("1.35G + 1.5W + 1.5Σ(φᵢ₀Qᵢ)");
                    }
                    else
                    {
                        Wcomb = 1.35 * P + 1.5 * (W + SumQ);
                        info.Add("1.35G + 1.5W");
                    }
                }
                else
                {
                    if (SumQ * W)
                    {
                        Wcomb = 1.35 * P + 1.5 * (W + SumQ);
                        info.Add("G + 1.5W + 1.5Σ(φᵢ₀Qᵢ)");
                    }
                    else
                    {
                        Wcomb = 1.35 * P + 1.5 * (W + SumQ);
                        info.Add("G + 1.5W");
                    }
                }
                Wcomb.type = W.type;
                Sd.Add(Wcomb);
            }

            //
            //INSTANTIATE CLASS
            //

            this.Sd = Sd.ToArray();
            this.info = info.ToArray();
            this.kmod = kmod.ToArray();
        }

    }
}

public class TypeInfo
{
    public double phi0;
    public double phi1;
    public double phi2;
    public string duration;

    public TypeInfo() { }
    public TypeInfo(string type)
    {
        if (type.Contains("P"))
        {
            phi0 = 0.7; phi1 = 0.5; phi2 = 0.3; duration = "perm";
            if (type.Contains("A")) { phi0 = 0.7; phi1 = 0.5; phi2 = 0.3; duration = "medium"; }
            if (type.Contains("B")) { phi0 = 0.7; phi1 = 0.5; phi2 = 0.3; duration = "medium"; }
            if (type.Contains("C")) { phi0 = 0.7; phi1 = 0.7; phi2 = 0.6; duration = "medium"; }
            if (type.Contains("D")) { phi0 = 0.7; phi1 = 0.7; phi2 = 0.6; duration = "medium"; }
            if (type.Contains("E")) { phi0 = 1; phi1 = 0.9; phi2 = 0.8; duration = "long"; }
            if (type.Contains("F")) { phi0 = 0.7; phi1 = 0.7; phi2 = 0.6; duration = "short"; }
            if (type.Contains("G")) { phi0 = 0.7; phi1 = 0.5; phi2 = 0.3; duration = "short"; }
            if (type.Contains("H")) { phi0 = 0; phi1 = 0; phi2 = 0; duration = "short"; }
            if (type.Contains("S")) { phi0 = 0.7; phi1 = 0.5; phi2 = 0.2; duration = "medium"; }
            if (type.Contains("W")) { phi0 = 0.6; phi1 = 0.2; phi2 = 0; duration = "short"; }
        }
    }



}
