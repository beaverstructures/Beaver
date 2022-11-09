
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeaverCore.Misc;
using BeaverCore.Materials;

namespace BeaverCore.Actions
{
    [Serializable]
    public class SLSCombinations
    {
        /// <summary>
        /// Given list of displacements sorts and calculates total displacements based on the Linear Analysis Formulation according to EC5 Section 2.2.3
        /// </summary>
        public List<Displacement> CharacteristicDisplacements;
        public List<Displacement> CreepDisplacements;
        public int SC; // service class


        public SLSCombinations() { }

        public SLSCombinations(List<Displacement> _wk, int _sc, Material _mat)
        {
            SC = _sc;
            _mat.Setkdef(SC);
            CalcDeflectionCombinations(_wk, _mat);
        }

        public void CharacteristicDeflectionCombination(List<List<Displacement>> SortedDisplacements)
        {
            List<Displacement> Displacements = new List<Displacement>();
            // CHARACTERISTIC-RARE COMBINATION
            // ΣG + P + Qk1 + Σ(φ₀Qkᵢ)
            Displacement Gravity = new Displacement();
            foreach (Displacement disp in SortedDisplacements[0])
            {
                // sums all gravity loads
                Gravity += disp;
            }

            // ΣG
            Gravity.combination = "G";
            Gravity.type = "P";
            Gravity.duration = "perm";
            Displacements.Add(Gravity);

            for (int primaryload = 1; primaryload < 4; primaryload++)
            {
                List<int> loadtypes = new List<int> { 1, 2, 3 };
                loadtypes.Remove(primaryload);
                for (int i = 1; i < SortedDisplacements[primaryload].Count; i++)
                {
                    // i=1 to skip 0 displacement case
                    // ΣG + Qk1
                    Displacement PrimaryDisplacement = SortedDisplacements[primaryload][i];
                    var cartesianproduct = Utils.CartesianProduct(new List<Displacement> { new Displacement() });



                    // Creates all possible combinations between IMPOSED LOADS: [Qa , Qh , Qa + Qh]
                    List<Displacement> ImposedDisplacements = SortedDisplacements.ElementAt(1).DeepClone();
                    ImposedDisplacements.RemoveAt(0); //removes 0 displacement case
                    if (primaryload == 1)
                    {
                        ImposedDisplacements.RemoveAt(i-1); //removes primary displacement case
                    }
                    var LiveCombinations = Enumerable.Range(1, (1 << ImposedDisplacements.Count) - 1).Select(index => ImposedDisplacements.Where((item, idx) => ((1 << idx) & index) != 0).ToList()).ToList();
                    ImposedDisplacements = new List<Displacement>();
                    foreach (var combo in LiveCombinations)
                    {
                        bool active = true;
                        Displacement sum = new Displacement();
                        foreach (var load in combo)
                        {
                            if (load.typeinfo.phi0 == 0) active = false;
                            else sum += load * load.typeinfo.phi0;
                            
                            // $$$ Accepting suggestions on how to improve this
                            // QX is set so that the phi0 is not accounted twice
                        }
                        if (active) ImposedDisplacements.Add(sum);
                    }
                    ImposedDisplacements.Insert(0, new Displacement());
                    if (primaryload == 1)
                    {
                        // Generates the CARTESIAN PRODUCT of secondary loads including other Imposed combinations
                        cartesianproduct = Utils.CartesianProduct(ImposedDisplacements,
                                                                        SortedDisplacements[loadtypes[0]],
                                                                        SortedDisplacements[loadtypes[1]]);
                    }
                    else
                    {
                        // Generates the CARTESIAN PRODUCT of secondary loads
                        cartesianproduct = Utils.CartesianProduct(ImposedDisplacements,
                                                                        SortedDisplacements[loadtypes[1]]);
                    }
                    Displacement PrimaryCombination = Gravity + PrimaryDisplacement;

                    // Generate Combinations according to EC5 Section 2.2.3
                    foreach (var product in cartesianproduct)
                    {
                        Displacement SecondaryDisplacement = new Displacement();
                        foreach (Displacement displacement in product)
                        {
                            if (displacement.type == "QX")
                            {
                                SecondaryDisplacement += displacement;
                            }
                            else
                            {
                                SecondaryDisplacement += displacement.typeinfo.phi0 * displacement;
                            }
                        }
                        Displacement SecondaryCombination = PrimaryCombination + SecondaryDisplacement;
                        SecondaryCombination.type = PrimaryDisplacement.type;
                        SecondaryCombination.duration = PrimaryDisplacement.duration;
                        Displacements.Add(SecondaryCombination);
                        
                    }
                }
            }

            CharacteristicDisplacements = Displacements;
        }

        public void LongTermDeflectionCombination(List<List<Displacement>> SortedDisplacements, Material mat)
        {
            List<Displacement> Displacements = new List<Displacement>();
            // QUASI-PERMANENT COMBINATION
            // EC5, Section 2.2.3, Eq. 2.2
            // ΣG∙(1+kdef) + P + Qk1∙φ₁∙(1+kdef∙φᵢ₂) + Σ(φᵢ₂Qkᵢ)∙(φᵢ₀ + kdef∙φᵢ₂)
            Displacement Gravity = new Displacement("P");
            foreach (Displacement disp in SortedDisplacements[0])
            {
                // sums all gravity loads
                Gravity += disp;
            }
            Gravity.type = "G";
          
            Displacement LongTermGravity = (1 + mat.kdef) * Gravity;
            Displacements.Add(LongTermGravity);

            for (int primaryload = 1; primaryload < 4; primaryload++)
            {
                List<int> loadtypes = new List<int> { 1, 2, 3 };
                loadtypes.Remove(primaryload);
                for (int i = 1; i < SortedDisplacements[primaryload].Count; i++)
                {
                    // i=1 to skip 0 displacement case
                    // ΣG + Qk1
                    Displacement PrimaryDisplacement = SortedDisplacements[primaryload][i];
                    PrimaryDisplacement *= (1 + PrimaryDisplacement.typeinfo.phi2 * mat.kdef);

                    var cartesianproduct = Utils.CartesianProduct(new List<Displacement> { new Displacement() });

                    // Creates all possible combinations between IMPOSED LOADS: [Qa , Qh , Qa + Qh]
                    List<Displacement> ImposedDisplacements = SortedDisplacements.ElementAt(1).DeepClone();
                    ImposedDisplacements.RemoveAt(0); //removes 0 displacement case
                    if (primaryload == 1)
                    {
                        ImposedDisplacements.RemoveAt(i-1); //removes primary displacement case
                    }
                    var ImposedCombinations = Enumerable.Range(1, (1 << ImposedDisplacements.Count) - 1).Select(index => ImposedDisplacements.Where((item, idx) => ((1 << idx) & index) != 0).ToList()).ToList();
                    ImposedDisplacements = new List<Displacement>();
                    foreach (var combo in ImposedCombinations)
                    {
                        bool active = true;
                        Displacement sum = new Displacement();
                        foreach (var load in combo)
                        {
                            if (load.typeinfo.phi0 == 0) active = false;
                            else sum += load * (load.typeinfo.phi0+mat.kdef*load.typeinfo.phi2);

                            // $$$ Accepting suggestions on how to improve this
                            // QX is set so that the phi0 is not accounted twice
                        }
                        if (active) ImposedDisplacements.Add(sum);
                    }
                    ImposedDisplacements.Insert(0, new Displacement());
                    if (primaryload == 1)
                    {
                        // Generates the CARTESIAN PRODUCT of secondary loads including other Imposed combinations
                        cartesianproduct = Utils.CartesianProduct(ImposedDisplacements,
                                                                        SortedDisplacements[loadtypes[0]],
                                                                        SortedDisplacements[loadtypes[1]]);
                    }
                    else
                    {
                        // Generates the CARTESIAN PRODUCT of secondary loads
                        cartesianproduct = Utils.CartesianProduct(ImposedDisplacements,
                                                                        SortedDisplacements[loadtypes[1]]);
                    }
                    Displacement PrimaryCombination = LongTermGravity + PrimaryDisplacement;

                    // Generate Combination according to EC5 Section 2.2.3

                    foreach (var product in cartesianproduct)
                    {
                        Displacement SecondaryDisplacement = new Displacement();
                        // ΣG + Qk1 + Σ(φ₀Qkᵢ)
                        foreach (Displacement displacement in product)
                        {
                            if (displacement.type == "QX")
                            {
                                SecondaryDisplacement += displacement;
                            }
                            else
                            {
                                SecondaryDisplacement += (displacement.typeinfo.phi0 + displacement.typeinfo.phi2 * mat.kdef) * displacement;
                            }
                        }
                        Displacement SecondaryCombination = PrimaryCombination + SecondaryDisplacement;
                        SecondaryCombination.type = PrimaryDisplacement.type;
                        SecondaryCombination.duration = PrimaryDisplacement.duration;
                        Displacements.Add(SecondaryCombination);
                    }
                }
            }

            CreepDisplacements = Displacements;

          
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

            List<List<Displacement>> SortedDisplacements = new List<List<Displacement>>
            {
                wk.Where(x => x.type.Contains("P")).ToList()
            };

            SortedDisplacements.Add(wk.Where(x => x.type.Contains("Q")).ToList());
            SortedDisplacements.Add(wk.Where(x => x.type.Contains("S")).ToList());
            SortedDisplacements.Add(wk.Where(x => x.type.Contains("W")).ToList());

            // generates 0 displacement for the cartesian product
            SortedDisplacements[1].Insert(0, new Displacement());
            SortedDisplacements[2].Insert(0, new Displacement());
            SortedDisplacements[3].Insert(0, new Displacement());

            CharacteristicDeflectionCombination(SortedDisplacements);
            LongTermDeflectionCombination(SortedDisplacements, mat);
        }
    }
}
