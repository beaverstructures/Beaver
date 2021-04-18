
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeaverCore.Misc;
using BeaverCore.Materials;
using Comb = Combinatorics.Collections;

namespace BeaverCore.Actions
{
    public class SLSCombinations
    {
        /// <summary>
        /// Given list of displacements sorts and calculates total displacements based on the Linear Analysis Formulation according to EC5 Section 2.2.3
        /// </summary>
        public List<Displacement> winst;
        public List<Displacement> wfin;
        public int SC; // service class


        public SLSCombinations() { }

        public SLSCombinations(List<Displacement> _wk, int _sc, Material _mat)
        {
            SC = _sc;
            _mat.Setkdef(SC);
            (winst, wfin) = CalcDeflectionCombinations(_wk,_mat);
        }

        public (List<Displacement> winst, List<Displacement> wfin) CalcDeflectionCombinations(List<Displacement> wk, Material mat)
        {
            // Linear Analysis Formulation according to EC5 Section 2.2.3
            // Possible combinations:
            // G
            // G+Q
            // G+W
            // G+Q+W -- alternating primary load


            // $$$ bug: code must point if there is an invalid input
            // $$$ Note: if the user inputs 2 load cases with the same type, algorithm will treat them as
            // $$$ separate loadcases. Isn't it easier to just sum everything from the same load ?
            // $$$ DOES NOT CONSIDER PRECAMBER

            // Sorts loads according to loadtype //
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

            List<List<Displacement>> Sorted_disps = new List<List<Displacement>>
            {
                wk.Where(x => x.type.Contains("P")).ToList()
            };

            Sorted_disps.Add(wk.Where(x => x.type.Contains("Q")).ToList());
            Sorted_disps.Add(wk.Where(x => x.type.Contains("S")).ToList());
            Sorted_disps.Add(wk.Where(x => x.type.Contains("W")).ToList());

            // generates 0 displacement for the cartesian product
            Sorted_disps[1].Insert(0, new Displacement());
            Sorted_disps[2].Insert(0, new Displacement());
            Sorted_disps[3].Insert(0, new Displacement());

            List<Displacement> displacements = new List<Displacement>();
            Displacement wG = new Displacement();

            // CHARACTERISTIC-RARE COMBINATION
            // ΣG + P + Qk1 + Σ(φ₀Qkᵢ)

            foreach (Displacement disp in Sorted_disps[0])
            {
                // sums all gravity loads
                wG += disp;
            }

            // ΣG
            wG.combination = "SLS-Characteristic";
            wG.type = "P";
            displacements.Add(wG);

            for (int primaryload = 1; primaryload < 4; primaryload++)
            {
                List<int> loadtypes = new List<int>{ 1, 2, 3 };
                loadtypes.Remove(primaryload);
                for (int i = 1; i < Sorted_disps[primaryload].Count; i++)
                {
                    // i=1 to skip 0 displacement case
                    // ΣG + Qk1
                    List<Displacement> disp1 = new List<Displacement> { Sorted_disps[primaryload][i] };
                    var cartesianproduct = Utils.CartesianProduct(new List<Displacement> { new Displacement() });


                    if (primaryload == 1)
                    {
                        // Creates all possible combinations between IMPOSED LOADS: [Qa , Qh , Qa + Qh]
                        List<Displacement> list = Sorted_disps.ElementAt(primaryload);
                        list.RemoveAt(0); //removes 0 displacement case
                        list.RemoveAt(i); //removes primary displacement case
                        var result = Enumerable.Range(1, (1 << list.Count) - 1).Select(index => list.Where((item, idx) => ((1 << idx) & index) != 0).ToList()).ToList();
                        list = new List<Displacement>();
                        foreach (var combo in result)
                        {
                            Displacement sum = new Displacement();
                            foreach (var load in combo)
                            {
                                sum += load * load.typeinfo.phi0;
                                sum.type = "QX";
                                // $$$ Accepting suggestions on how to improve this
                                // QX is set so that the phi0 is not accounted twice
                            }
                            list.Add(sum);
                        }
                        list.Insert(0, new Displacement());

                        // Generates the CARTESIAN PRODUCT of secondary loads including other Imposed combinations
                        cartesianproduct = Utils.CartesianProduct(disp1,
                                                                        list,
                                                                        Sorted_disps[loadtypes[0]],
                                                                        Sorted_disps[loadtypes[1]]);
                    }
                    else
                    {
                        // Generates the CARTESIAN PRODUCT of secondary loads
                        cartesianproduct = Utils.CartesianProduct(disp1,
                                                                        Sorted_disps[loadtypes[0]],
                                                                        Sorted_disps[loadtypes[1]]);
                    }


                    // Sum displacements inside cartesian products
                    Displacement disp2 = new Displacement();
                    foreach (var product in cartesianproduct)
                    {
                        // ΣG + Qk1 + Σ(φ₀Qkᵢ)
                        foreach (var item in product)
                        {
                            // calculates Σ(φ₀Qkᵢ)
                            disp2 += item * item.typeinfo.phi0;
                        }
                        displacements.Add(wG + disp1[0] + disp2);
                    }
                }
            }

            winst = displacements;
            displacements = new List<Displacement>();

            // QUASI-PERMANENT COMBINATION
            // EC5, Section 2.2.3, Eq. 2.2
            // ΣG∙(1+kdef) + P + Qk1∙φ₁∙(1+kdef∙φᵢ₂) + Σ(φᵢ₂Qkᵢ)∙(φᵢ₀ + kdef∙φᵢ₂)

            // ΣG∙(1+kdef)
            wG.combination = "SLS-QuasiPermanent";
            displacements.Add(wG * (1 + mat.kdef));

            for (int primaryload = 1; primaryload < 4; primaryload++)
            {
                List<int> loadtypes = new List<int> { 1, 2, 3 };
                loadtypes.Remove(primaryload);
                for (int i = 1; i < Sorted_disps[primaryload].Count; i++)
                {
                    // ΣG∙(1+kdef) + Qk1∙φ₁∙(1+kdef∙φᵢ₂)
                    List<Displacement> disp1 = new List<Displacement> { Sorted_disps[primaryload][i] };

                    // Generates the CARTESIAN PRODUCT of secondary loads
                    var cartesianproduct = Utils.CartesianProduct(disp1,
                                                                    Sorted_disps[loadtypes[0]],
                                                                    Sorted_disps[loadtypes[1]]);

                    // Sum displacements inside cartesian products
                    Displacement disp2 = new Displacement();
                    foreach (var product in cartesianproduct)
                    {
                        // ΣG∙(1+kdef) + Qk1∙φ₁∙(1+kdef∙φᵢ₂) + Σ(φᵢ₂Qkᵢ)∙(φᵢ₀ + kdef∙φᵢ₂)
                        foreach(var item in product)
                        {
                            // calculates Σ(φᵢ₂Qkᵢ)∙(φᵢ₀ + kdef∙φᵢ₂)
                            disp2 += item * item.typeinfo.phi2 *(item.typeinfo.phi0 + mat.kdef);
                        }
                        displacements.Add(  wG * (1 + mat.kdef)
                                            + disp1[0] * disp1[0].typeinfo.phi1 * (1 + mat.kdef * disp1[0].typeinfo.phi2)
                                            + disp2);
                    }
                }
            }

            wfin = displacements;
            return (winst, wfin);
        }
    }    
}
