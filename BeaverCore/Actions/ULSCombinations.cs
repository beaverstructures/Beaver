using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeaverCore.Misc;

namespace BeaverCore.Actions
{
    public enum FilterType
    {
        ByLoadDuration,
        ByLoadType,
        Total
    }

    public class ULSCombinations
    {
        public Force[] Sd;                          // List of design forces
        public int SC;                              // Service Class 
        public ULSCombinations() { }

        public ULSCombinations(List<Force> Sk, int sc)
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
            P.duration = new TypeInfo(P.type).duration;
            P.combination = "1.35G";
            Sd.Add(P);

            //
            //ACIDENTAL LOADING
            // $$$ Shouldnt it be named IMPOSED or VARIABLE loading instead of ACCIDENTAL?
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
                // $$$ doubt: Should we consider the possibility of combining oposite direction variable loads?
                Force A = 1.35 * P + 1.5 * (Qmain + SumQi);
                A.type = Qmain.type;
                A.duration = new TypeInfo(A.type).duration;
                A.combination = "1.35G + 1.5 " + A.type + " + 1.5Σ(φᵢ₀Qᵢ)";
                Sd.Add(A);
                foreach (Force w in SWk)
                {
                    Force B = A + 0.6 * w;
                    B.combination = "1.35G + 1.5 " + A.type + " + 1.5(Σ(φᵢ₀Qᵢ)+0.6W)";
                }
            }

            //
            //WIND LOADING
            //

            Force SumQ = new Force();
            foreach (Force Q in SQk)
            {
                TypeInfo t = new TypeInfo(Q.type);
                SumQ += Q * t.phi0;
            }
            foreach (Force W in SWk)
            {
                Force Wcomb = new Force();
                string output = "";
                if (P * W) // $$$ doubt, what does that * means?
                {
                    if (SumQ * W) // $$$ essa notação não é intuitiva
                    {
                        Wcomb = 1.35 * P + 1.5 * (W + SumQ);

                        output = "1.35G + 1.5W + 1.5Σ(φᵢ₀Qᵢ)";
                    }
                    else
                    {
                        Wcomb = 1.35 * P + 1.5 * (W + SumQ);
                        output = "1.35G + 1.5W";
                    }
                }
                else // $$$ Wcomb and output equations here are not matching, Wcomb should contain 1.0*P and not 1.35*P.
                    // $$$ Also, I believe that SumQ may be disconsidered in this combination when they are favorable to the analysis such as Permanent loads 
                {
                    if (SumQ * W)
                    {
                        Wcomb = 1.35 * P + 1.5 * (W + SumQ);
                        output = "G + 1.5W + 1.5Σ(φᵢ₀Qᵢ)";
                    }
                    else
                    {
                        Wcomb = 1.35 * P + 1.5 * (W + SumQ);
                        output = "G + 1.5W";
                    }
                }
                Wcomb.type = W.type;
                Wcomb.duration = new TypeInfo(W.type).duration;
                Wcomb.combination = output;
                Sd.Add(Wcomb);
            }

            //
            //INSTANTIATE CLASS
            //

            this.Sd = Sd.ToArray();                             // $$$ evitar usar nome de variável com nome de field pq confunde a leitura
        }
        public List<Force> CriticalForces(List<Force> f)
        {
            int[] idxmax = new int[] { 0, 0, 0, 0, 0, 0 };
            int[] idxmin = new int[] { 0, 0, 0, 0, 0, 0 };
            Force Max = new Force();
            Force Min = new Force();
            int cont = 0;
            foreach (Force force in f)
            {
                int idx = 0;
                double fkmod = Utils.KMOD(SC, force.duration);
                foreach (KeyValuePair<string, double> IF in force.InternalForces)
                {
                    double minkmod = Utils.KMOD(SC, f[idxmin[idx]].duration);
                    if (IF.Value / fkmod <= Min.InternalForces[IF.Key] / minkmod && IF.Value < 0)
                    {
                        Min.InternalForces[IF.Key] = IF.Value;
                        idxmin[idx] = cont;
                    }
                    double maxkmod = Utils.KMOD(SC, f[idxmin[idx]].duration);
                    if (IF.Value / fkmod >= Max.InternalForces[IF.Key] / maxkmod && IF.Value > 0)
                    {
                        Max.InternalForces[IF.Key] = IF.Value;
                        idxmax[idx] = cont;
                    }
                    idx++;
                }
                cont++;
            }
            List<int> finalidx = new List<int>();
            foreach (int idx in idxmax)
            {
                if (finalidx.Exists(x => x == idx) == false && f[idx].InternalForces.Values.ToList().Exists(x => x != 0))
                {
                    finalidx.Add(idx);
                }
            }
            foreach (int idx in idxmin)
            {
                if (finalidx.Exists(x => x == idx) == false && f[idx].InternalForces.Values.ToList().Exists(x => x != 0))
                {
                    finalidx.Add(idx);
                }
            }
            List<Force> result = new List<Force>();
            foreach (int idx in finalidx)
            {
                result.Add(f[idx]);
            }
            return result;
        }
        public void FilterCombinations(int type)
        {

            List<Force> Sd = new List<Force>();
            List<double> kmod = new List<double>();
            List<string> info = new List<string>();

            //
            //BY LOAD DURATION
            if (type == 0)
            {
                List<string> types = new List<string>() { "perm", "long", "medium", "short" };
                List<List<Force>> Sp = new List<List<Force>>();
                foreach (string t in types)
                {
                    if (this.Sd.Where(x => x.type.Contains(t)).ToList().Count > 0)
                    {
                        Sp.Add(this.Sd.Where(x => x.type.Contains(t)).ToList());
                    }
                }
                foreach (List<Force> Lf in Sp)
                {
                    List<Force> aux = CriticalForces(Lf);
                    foreach (Force f in aux)
                    {
                        Sd.Add(f);
                    }
                }
                this.Sd = Sd.ToArray();
            }
            //
            //BY LOAD TYPE
            //
            if (type == 1)
            {
                List<string> types = new List<string>() { "P", "A", "B", "C", "D", "E", "F", "G", "H", "S", "W" };
                List<List<Force>> Sp = new List<List<Force>>();
                foreach (string t in types)
                {
                    if (this.Sd.Where(x => x.type.Contains(t)).ToList().Count > 0)
                    {
                        Sp.Add(this.Sd.Where(x => x.type.Contains(t)).ToList());
                    }
                }
                foreach (List<Force> Lf in Sp)
                {
                    List<Force> aux = CriticalForces(Lf);
                    foreach (Force f in aux)
                    {
                        Sd.Add(f);
                    }
                }
                this.Sd = Sd.ToArray();
            }
            //
            //TOTAL
            //
            if (type == 2)
            {
                Sd = CriticalForces(new List<Force>(this.Sd));
                this.Sd = Sd.ToArray();
            }
        }

    }
}

