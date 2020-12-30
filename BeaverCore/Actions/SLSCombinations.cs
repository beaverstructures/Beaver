using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeaverCore.Misc;
using BeaverCore.Materials;

namespace BeaverCore.Actions
{
    public class SLSCombinations
    {
        // variables
        public List<Displacement> w_inst;
        public List<Displacement> w_fin;
        public List<Displacement> wnet;
        public int SC;


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

            List<Displacement> w_inst_list = new List<Displacement>();
            List<Displacement> w_primary_final_list = new List<Displacement>();
            List<Displacement> w_secondary_final_list = new List<Displacement>();

            Displacement wG_inst = new Displacement("P");
            Displacement wG_fin = new Displacement("P");

            foreach (string load in loadList)
            {
                Displacement sum_disp = new Displacement();
                foreach (Displacement disp in wk.Where(x => x.type.Contains(load)).ToList())
                {
                    sum_disp += disp;
                }
                if (load == "P")
                {
                    wG_inst = sum_disp;
                    wG_fin = sum_disp * (1 + mat.kdef);

                    w_inst_list.Add(sum_disp);
                    w_primary_final_list.Add(sum_disp * (1 + mat.kdef));
                    w_secondary_final_list.Add(new Displacement());
                }
                else
                {
                    w_inst_list.Add(sum_disp);
                    w_primary_final_list.Add(sum_disp * (1 + sum_disp.typeinfo.phi2 * mat.kdef));
                    w_secondary_final_list.Add(sum_disp * (sum_disp.typeinfo.phi0 + sum_disp.typeinfo.phi2 * mat.kdef));
                }
            }

            // CHARACTERISTIC COMBINATION
        }
    }    
}
