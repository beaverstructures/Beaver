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
    /// <summary>
    /// Defines the array of combinated forces according to Eurocode 0, Annex A1
    /// </summary>
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
        /// <summary>
        /// Generates all possible combinations (EC0 Eq. 6.9-6.10).
        /// Takes account for favourable and unfavourable actions
        /// according to EC0 Table A1.2(B).
        /// <param name="Sk"></param>
        /// 

        public void DesignAction(List<Force> LoadcaseForce)
        {
            List<string> loadList = new List<string>()
                                    {"P",
                                    "QA",
                                    "QB",
                                    "QC",
                                    "QD",
                                    "QE",
                                    "QE",
                                    "QG",
                                    "QH",
                                    "S",
                                    "W"};

            Force Gravity = new Force();

            // Persistent Combination
            // ΣγgP + γq(Qk1 + Σ(φ₀Qkᵢ))

            //Add Gravity Only Combinations
            foreach (Force force in LoadcaseForce.Where(x => x.type.Contains("P")).ToList())
            {
                // sums all gravity loads
                Gravity += force;
            }
            Force GravityForce = 1.35 * Gravity;
            GravityForce.combination = "ΣγgP";
            GravityForce.type = "P";

            List<Force> forces = new List<Force>() { GravityForce };

            List<Force> Gravities = new List<Force>() { Gravity, 1.35 * Gravity };

            forces.Add(1.35 * Gravity);

            List<List<Force>> SortedForces = new List<List<Force>>()
            {
                Gravities
            };


            //Prepare Live, Wind and Snow sublists
            SortedForces.Add(LoadcaseForce.Where(x => x.type.Contains("Q")).ToList());
            SortedForces.Add(LoadcaseForce.Where(x => x.type.Contains("S")).ToList());
            SortedForces.Add(LoadcaseForce.Where(x => x.type.Contains("W")).ToList());

            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < SortedForces[i].Count; j++)
                {
                    SortedForces[i][j] *= 1.5;
                }
            }
            // generates null force for the cartesian product
            // (all possible combinations between values)
            SortedForces[1].Insert(0, new Force());
            SortedForces[2].Insert(0, new Force());
            SortedForces[3].Insert(0, new Force());



            for (int primaryload = 1; primaryload < 4; primaryload++)
            {
                List<int> loadtypes = new List<int> { 1, 2, 3 };
                loadtypes.Remove(primaryload);
                for (int i = 1; i < SortedForces[primaryload].Count; i++)
                {
                    // i=1 to skip null force case
                    // ΣγgP + γqQk1
                    Force PrimaryForce = SortedForces[primaryload][i];
                    var cartesianproduct = Utils.CartesianProduct(new List<Force> { new Force() });

                    // Creates all possible combinations between LIVE LOADS: [Qa , Qh , Qa + Qh]
                    List<Force> ImposedForces = SortedForces.ElementAt(1);
                    ImposedForces.RemoveAt(0); //removes null case
                    if (primaryload == 1)
                    {
                        ImposedForces.RemoveAt(i); //removes primary case if live load
                    }
                    List<List<Force>> SecondaryImposedCombinations = Enumerable.Range(1, (1 << ImposedForces.Count) - 1).Select(index => ImposedForces.Where((item, idx) => ((1 << idx) & index) != 0).ToList()).ToList();
                    List<Force> SecondaryImposedForces = new List<Force>();
                    foreach (List<Force> combo in SecondaryImposedCombinations)
                    {
                        Force sum = new Force();
                        foreach (Force load in combo)
                        {
                            sum += load * load.typeinfo.phi0;
                            sum.type = "QX";
                            // $$$ Accepting suggestions on how to improve this
                            // QX is set so that the phi0 is not accounted twice
                        }
                        SecondaryImposedForces.Add(sum);
                    }
                    SecondaryImposedForces.Insert(0, new Force());

                    if (primaryload == 1)
                    {
                        // Generates the CARTESIAN PRODUCT of live secondary loads including other variable combinations (Snow and Wind)
                        cartesianproduct = Utils.CartesianProduct(SecondaryImposedForces,
                                                                     SortedForces[loadtypes[0]],
                                                                     SortedForces[loadtypes[1]]);
                    }
                    else
                    {
                        //Generates the CARTESION PRODUCT of live secondary loads including other variable combinations (Snow or Wind)
                        cartesianproduct = Utils.CartesianProduct(SecondaryImposedForces,
                                                                     SortedForces[loadtypes[1]]);
                    }


                    
                    Force SecondaryForce = new Force();
                    foreach (var product in cartesianproduct)
                    {
                        // Sum forces inside cartesian products
                        foreach (Force force in product)
                        {
                            if (force.type == "QX")
                            {
                                SecondaryForce += force;
                            }
                            else
                            {
                                SecondaryForce += force.typeinfo.phi0 * force;
                            }
                            
                        }
                        //Sum fabourable and unfabourable combinations (EC0 Table A1.2)
                        Force FavourableForce = Gravity + SecondaryForce;
                        FavourableForce.combination = "G + 1.5Σ(φᵢ₀Qᵢ)";
                        FavourableForce.type = PrimaryForce.type;
                        Force UnfavourableForce = 1.35 * Gravity + SecondaryForce;
                        UnfavourableForce.combination = "1.35G + 1.5Σ(φᵢ₀Qᵢ)";
                        UnfavourableForce.type = PrimaryForce.type;
                        forces.Add(FavourableForce);
                        forces.Add(UnfavourableForce);
                        FavourableForce += PrimaryForce;
                        FavourableForce.combination = "G + 1.5 " + PrimaryForce.type + " + 1.5Σ(φᵢ₀Qᵢ)";
                        UnfavourableForce += PrimaryForce;
                        UnfavourableForce.combination = "1.35G + 1.5 " + PrimaryForce.type + " + 1.5Σ(φᵢ₀Qᵢ)";
                        forces.Add(FavourableForce);
                        forces.Add(UnfavourableForce);

                    }
                }
            }
            Sd = new List<Force>(forces).ToArray();
        }

        /// <summary>
        /// Defines the critical combinations.
        /// This function is used to allow to operate with a reduced number of combinations
        /// by determining the critical values for each internal force (N,V,M) for both positive
        /// and negative values. 
        /// Please be carefull when using this. It can help a lot to reduce computation time, 
        /// but the final analysis must be made always considering all possible combinations.
        /// </summary>
        /// <param name="forces"></param>
        /// <returns></returns>
        public List<Force> CriticalForces(List<Force> forces)
        {
            int[] idxmax = new int[] { 0, 0, 0, 0, 0, 0 };
            int[] idxmin = new int[] { 0, 0, 0, 0, 0, 0 };
            List<double> Max = new List<double>() { 0, 0, 0, 0, 0, 0 };
            List<double> Min = new List<double>() { 0, 0, 0, 0, 0, 0 };
            int cont = 0;
            List<List<double>> force_lists = new List<List<double>>();
            foreach (Force force in forces)
            {
                List<double> force_list = force.ToList();
                force_lists.Add(force_list);
                int idx = 0;
                double fkmod = Utils.KMOD(SC, force.duration);
                for (int i = 0; i < 6; i++)
                {
                    double internalforce = force_list[i];
                    double minkmod = Utils.KMOD(SC, forces[idxmin[i]].duration);
                    if (internalforce / fkmod <= Min[i] / minkmod && internalforce < 0)
                    {
                        Min[i] = internalforce;
                        idxmin[i] = cont;
                    }
                    double maxkmod = Utils.KMOD(SC, forces[idxmin[i]].duration);
                    if (internalforce / fkmod >= Max[i] / maxkmod && internalforce > 0)
                    {
                        Max[i] = internalforce;
                        idxmax[i] = cont;
                    }
                }
                cont++;
            }
            List<int> finalidx = new List<int>();

            foreach (int idx in idxmax)
            {
                if (finalidx.Exists(x => x == idx) == false && force_lists[idx].Exists(x => x != 0))
                {
                    finalidx.Add(idx);
                }
            }
            foreach (int idx in idxmin)
            {
                if (finalidx.Exists(x => x == idx) == false && force_lists[idx].Exists(x => x != 0))
                {
                    finalidx.Add(idx);
                }
            }
            List<Force> result = new List<Force>();
            foreach (int idx in finalidx)
            {
                result.Add(forces[idx]);
            }
            return result;
        }


        /// <summary>
        /// Filter the combination using the CriticalForce function.
        /// It can filter by load duration, load type and for all combinations.
        /// </summary>
        /// <param name="type"></param>
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

    }
}


