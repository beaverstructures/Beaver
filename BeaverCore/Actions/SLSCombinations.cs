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
        // variables
        public int SC; // service class


        public SLSCombinations() { }

        public SLSCombinations(List<Displacement> _wk, int _sc, Material _mat)
        {
            SC = _sc;
            _mat.Setkdef(SC);
            CalcDeflectionCombinations(_wk,_mat);
        }

        public void CalcDeflectionCombinations(List<Displacement> wk, Material mat)
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

            // CALCULATES DEFLECTION ACCORDING TO LOAD TYPE //
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

            List<List<Displacement>> Sorted_disps = new List<List<Displacement>>();

            Sorted_disps.Add(wk.Where(x => x.type.Contains("P")).ToList());

            // Creates all possible combinations between IMPOSED LOADS: [Qa , Qh , Qa + Qh]
            List<Displacement> list = wk.Where(x => x.type.Contains("Q")).ToList();
            var result = Enumerable.Range(1, (1 << list.Count) - 1).Select(index => list.Where((item, idx) => ((1 << idx) & index) != 0).ToList());
            list = new List<Displacement>();
            foreach (var combo in result)
            {
                Displacement sum = new Displacement();
                sum.type = "Q";
                // $$$ Error: does not consider phi0 or phi2 in the sum
                foreach (var load in combo)
                {
                    sum += load;
                }
                list.Add(sum);
            }

            Sorted_disps.Add(list);
            Sorted_disps.Add(wk.Where(x => x.type.Contains("S")).ToList());
            Sorted_disps.Add(wk.Where(x => x.type.Contains("W")).ToList());

            // generates 0 displacement for the cartesian product
            Sorted_disps[1].Add(new Displacement());
            Sorted_disps[2].Add(new Displacement());
            Sorted_disps[3].Add(new Displacement());

            List<Displacement> displacements = new List<Displacement>();
            Displacement displacement_sum = new Displacement();
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
            displacement_sum += wG;
            displacements.Add(displacement_sum);
            
            for(int primaryload = 1; primaryload < 4; primaryload++)
            {
                for (int i = 1; i < Sorted_disps[primaryload].Count; i++)
                {
                    // ΣG + Qk1
                    Displacement disp1 = Sorted_disps[primaryload][i];
                    displacements.Add(wG + disp1);

                    // Remaining Loads
                    Sorted_disps.RemoveAt(0);
                    Sorted_disps[primaryload].RemoveAt(i);

                    // Generates the CARTESIAN PRODUCT of secondary loads
                    var cartesianproduct = Utils.CartesianProduct(Sorted_disps[1], Sorted_disps[2], Sorted_disps[3]);

                    // Sum displacements inside cartesian products
                    Displacement disp2 = new Displacement();
                    foreach (var product in cartesianproduct)
                    {
                        // ΣG + Qk1 + Σ(φ₀Qkᵢ)
                        disp2.Sum_SLS_char(product); // Sum_SLS_char() method to be implemented in displacement class
                        displacements.Add(wG + disp1 + disp2);
                    }
                }
            }

            // QUASI-PERMANENT COMBINATION
            // ΣG∙(1+kdef) + P + Qk1∙φ₁∙(1+kdef∙φᵢ₂) + Σ(φᵢ₂Qkᵢ)∙(φᵢ₀ + kdef∙φᵢ₂)
            
            // ΣG∙(1+kdef)
            wG.combination = "SLS-QuasiPermanent";
            displacement_sum = wG * (1 + mat.kdef);
            displacements.Add(displacement_sum);                                            

            for (int primaryload = 1; primaryload < 4; primaryload++)
            {
                for (int i = 1; i < Sorted_disps[primaryload].Count; i++)
                {
                    // ΣG∙(1+kdef) + Qk1∙φ₁∙(1+kdef∙φᵢ₂)
                    Displacement disp1 = Sorted_disps[primaryload][i];
                    displacements.Add(wG*(1 + mat.kdef) + disp1*disp1.typeinfo.phi1*(1+mat.kdef*disp1.typeinfo.phi2));

                    // Remaining Loads
                    Sorted_disps.RemoveAt(0);
                    Sorted_disps[primaryload].RemoveAt(i);

                    // Cartesian product of secondary loads
                    var cartesianproduct = Utils.CartesianProduct(Sorted_disps[1], Sorted_disps[2], Sorted_disps[3]);

                    // Sum displacements inside cartesian products
                    Displacement disp2 = new Displacement();
                    foreach (var product in cartesianproduct)
                    {
                        // ΣG∙(1+kdef) + Qk1∙φ₁∙(1+kdef∙φᵢ₂) + Σ(φᵢ₂Qkᵢ)∙(φᵢ₀ + kdef∙φᵢ₂)
                        disp2.Sum_SLS_QP(product, mat.kdef); // Sum_SLS_QP() method to be implemented in displacement class
                        displacements.Add(  wG * (1 + mat.kdef) 
                                        + disp1 * disp1.typeinfo.phi1 * (1 + mat.kdef * disp1.typeinfo.phi2)
                                        + disp2);
                    }
                }
            }

            Displacement Result = displacements.Max();
        }
    }    
}
