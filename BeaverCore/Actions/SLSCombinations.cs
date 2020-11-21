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

            // Classify initial data by load type
            List<Displacement> wGk = wk.Where(x => x.type.Contains("P")).ToList();
            List<Displacement> wQk = wk.Where(x => x.type.Contains("Q")).ToList();
            List<Displacement> wWk = wk.Where(x => x.type.Contains("W")).ToList();
            // $$$ bug: code must point if there is an invalid input
            // $$$ Note: if the user inputs 2 load cases with the same type, algorithm will treat them as
            // $$$ separate loadcases. Isn't it easier to just sum everything from the same load ?

            List<double> kmod = new List<double>();
            List<string> info = new List<string>();

            // CALCULATES DEFLECTION ACCORDING TO LOAD TYPE //
            Displacement wGinst = new Displacement("P");    // permanent instantaneous
            Displacement wGfinal = new Displacement("P");   // permanent after creep

            Displacement wQinst = new Displacement("Q");    // accidental instantaneous
            Displacement wQ1final = new Displacement("Q");  // primary accidental after creep
            Displacement wQ2final = new Displacement("Q");  // secondary accidental after creep

            Displacement wWinst = new Displacement("W");    // primary wind instantaneous
            Displacement wW1final = new Displacement("W");   // wind primary after creep (EC5 requires but doesn't make sense)
            Displacement wW2final = new Displacement("W");   // wind secondary after creep (EC5 requires but doesn't make sense)

            //Displacement wC = new Displacement();         // precamber not implemented

            //METHOD 1: merges loads with same type and adds creep considerations

            foreach (Displacement disp in wGk)
            {
                wGinst += disp;
                wGfinal += disp * (1 + mat.kdef);
            }
            foreach (Displacement disp in wQk)
            {
                wQinst += disp;
                wQ1final += disp * (1 + mat.kdef);
                wQ2final += disp * (disp.typeinfo.phi0 + mat.kdef* disp.typeinfo.phi0);
            }
            foreach (Displacement disp in wWk)
            {
                wWinst += disp;
                wW1final += disp;
                wW2final += disp * (disp.typeinfo.phi0);
            }

            // CHARACTERISTIC COMBINATION
            //Instantaneous deflection
            w_inst.Add(wGinst);                                         // G
            w_inst.Add(wGinst + wQinst);                                // G + Q1
            w_inst.Add(wGinst + wWinst);                                // G + W
            w_inst.Add(wGinst + wQinst + wWinst*wWinst.typeinfo.phi0);  // G + Q1 + φ₀W
            w_inst.Add(wGinst + wQinst*wQinst.typeinfo.phi0 + wWinst);  // G + φ₀Q1 + W
            //Long-term deflection
            w_fin.Add(wGfinal);
            w_fin.Add(wGfinal + wQ1final);
            w_fin.Add(wGfinal + wW1final);
            w_fin.Add(wGfinal + wQ1final + wW2final * wW2final.typeinfo.phi0);
            w_fin.Add(wGfinal + wQ2final * wQ1final.typeinfo.phi0 + wW1final);

            // FREQUENT COMBINATION
            //Instantaneous deflection
            w_inst.Add(wGinst);                                                                             // G            
            w_inst.Add(wGinst + wQinst * wQinst.typeinfo.phi1);                                             // G + φ₁Q1
            w_inst.Add(wGinst + wWinst * wQinst.typeinfo.phi1);                                             // G + φ₁W
            w_inst.Add(wGinst + wQinst * wQinst.typeinfo.phi1 + wWinst * wWinst.typeinfo.phi2);             // G + φ₁Q1 + φ₂W
            w_inst.Add(wGinst + wQinst * wQinst.typeinfo.phi2 + wWinst);                                    // G + φ₂Q1 + φ₁W
            //Long-term deflection
            w_fin.Add(wGfinal);                                                                             // Gfin            
            w_fin.Add(wGfinal + wQ1final * wQ1final.typeinfo.phi1);                                         // Gfin + φ₁Q1fin
            w_fin.Add(wGfinal + wW1final * wW1final.typeinfo.phi1);                                         // Gfin + φ₁Wfin
            w_fin.Add(wGfinal + wQ1final * wQ1final.typeinfo.phi1 + wW2final * wW2final.typeinfo.phi2);     // Gfin + φ₁Q1fin + φ₂Wfin
            w_fin.Add(wGfinal + wQ2final * wQ2final.typeinfo.phi2 + wW1final);                              // Gfin + φ₂Q2fin + φ₁Wfin

            // QUASI-PERMANENT COMBINATION
            //Instantaneous deflection
            w_inst.Add(wGinst);                                                                             // G            
            w_inst.Add(wGinst + wQinst * wQinst.typeinfo.phi2);                                             // G + φ₂Q1
            //Long-term deflection
            w_fin.Add(wGfinal);                                                                             // Gfin            
            w_fin.Add(wGfinal + wQ1final * wQ1final.typeinfo.phi2);                                         // Gfin + φ₂Q1fin

            // METHOD 2: treating loads separately. Still under implementation.

            //// Permanent loading
            //foreach (Displacement disp in wGk)
            //{
            //    wGinst += disp;
            //}
            //wGinst.combination = "Ginst";
            //w_inst.Add(wGinst);
            //wGinst.combination = "Gfin";
            //w_fin.Add(wGinst * (1+mat.kdef));

            //// Leading Accidental loading
            //List<Displacement> wQ = new List<Displacement>();
            //for (int i = 0; i < wQk.Count; i++)
            //{
            //    Displacement wQ1 = wQk[i];

            //    // Calculates characteristic load combination (see EC0 Section 6.5.3)
            //    List<Displacement> wQi = new List<Displacement>(wQk);
            //    wQi.RemoveAt(i);
            //    Displacement sum_wQi = new Displacement("Q");
            //    Displacement sum_wQi_fin = new Displacement("Q");

            //    foreach (Displacement secondaryload in wQi)
            //    {
            //        TypeInfo t = secondaryload.typeinfo;
            //        sum_wQi += secondaryload * t.phi0;
            //        sum_wQi_fin = secondaryload * t.phi0*(t.phi0+t.phi2*mat.kdef);
            //    }
            //    // G + Qk1 + Σ(φᵢ₀Qkᵢ)
            //    wGinst.combination = "Qinst";
            //    w_inst.Add(wGinst + (wQ1 + sum_wQi));
            //    wGinst.combination = "Qfin";
            //    // G*(1+kdef) + Qk1 *(1+kdef*φᵢ₂) + Σ(φᵢ₀Qkᵢ)*(φᵢ₀+kdef*φᵢ₂ )
            //    w_fin.Add(wGinst * (1 + mat.kdef) + wQ1 * (1 + mat.kdef*wQ1.typeinfo.phi2) + sum_wQi_fin);


            //    foreach (Displacement secondary_windload in wWk)
            //    {
            //        TypeInfo t = secondary_windload.typeinfo;
            //        // G + Qk1 + Σ(φᵢ₀Qkᵢ) + φᵢ₀Qwᵢ
            //        w_inst.Add(wGinst + (wQ1 + sum_wQi + secondary_windload * t.phi0));
            //        // G*(1+kdef) + Qk1*(1+kdef*φᵢ₂) + Σ(φᵢ₀Qkᵢ)*(φᵢ₀+kdef*φᵢ₂) + φᵢ₀Qwᵢ*(φᵢ₀+kdef*φᵢ₂)
            //        w_fin.Add(wGinst * (1 + mat.kdef) + (wQ1 * (1 + mat.kdef * wQ1.typeinfo.phi2) + sum_wQi_fin + secondary_windload * t.phi0 * (1+mat.kdef*t.phi2));
            //    }


            //    // Calculates frequent load combination (see EC0 Section 6.5.3)
            //    sum_wQi = new Displacement("Q");
            //    sum_wQi_fin = new Displacement("Q");
            //    TypeInfo t1 = new TypeInfo(wQ1.type);
            //    foreach (Displacement secondaryload in wQi)
            //    {
            //        TypeInfo t = new TypeInfo(secondaryload.type);
            //        sum_wQi += secondaryload * t.phi2;
            //        sum_wQi_fin += secondaryload * t.phi2 * (t.phi0 + t.phi2 * mat.kdef);
            //    }
            //    // G + φᵢ₁Qk1 + Σ(φᵢ₂Qkᵢ)
            //    w_inst.Add(wGinst + (wQ1 * t1.phi1 + sum_wQi));
            //    // G*(1+kdef) + Qk1*φ₁*(1+kdef*φᵢ₂) + Σ(φᵢ₂Qkᵢ)*(φᵢ₀+kdef*φᵢ₂ )
            //    w_fin.Add(wGinst*(1+mat.kdef) + (wQ1 * t1.phi1)* (1 + mat.kdef * t1.phi2) + sum_wQi_fin);

            //    foreach (Displacement secondary_windload in wWk)
            //    {
            //        TypeInfo t = new TypeInfo(secondary_windload.type);
            //        // G + φᵢ₁Qk1 + Σ(φᵢ₂Qkᵢ) + φᵢ₂Qwᵢ
            //        w_inst.Add(wGinst + (wQ1 * t1.phi1 + sum_wQi + secondary_windload * t.phi2));
            //        // G*(1+kdef) + Qk1*φ₁*(1+kdef*φᵢ₂) + {Σ(φᵢ₂Qkᵢ) + φᵢ₂Qwᵢ}*(φᵢ₀+kdef*φᵢ₂ )
            //        w_fin.Add(  wGinst * (1 + mat.kdef) +
            //                    wQ1 * t1.phi1 * (
            //                    )
            // $$ STOPPED HERE
            //    }


            //    // Calculates quasi-permanent load combination (see EC0 Section 6.5.3)
            //    sum_wQi = new Displacement("Q");
            //    foreach (Displacement secondaryload in wQk)
            //    {
            //        TypeInfo t = new TypeInfo(secondaryload.type);
            //        sum_wQi += secondaryload * t.phi2;
            //    }
            //    // G + Σ(φᵢ₂Qkᵢ)
            //    wQ.Add(wGinst + (wQ1 + sum_wQi));

            //    foreach (Displacement secondary_windload in wWk)
            //    {
            //        TypeInfo t = new TypeInfo(secondary_windload.type);
            //        // G + Σ(φᵢ₀Qk₂) + φᵢ₂Qwᵢ
            //        wQ.Add(wGinst + (wQ1 + sum_wQi + secondary_windload * t.phi2));
            //    }
            //}

            //// Leading Wind Load
            //List<Displacement> wW = new List<Displacement>();
            //for (int i = 0; i < wWk.Count; i++)
            //{
            //    Displacement wW1 = wWk[i];
            //    TypeInfo t1 = new TypeInfo(wW1.type);
            //    Displacement sum_wQi = new Displacement("Q");

            //    foreach (Displacement secondaryload in wQk)
            //    {
            //        TypeInfo t = new TypeInfo(secondaryload.type); //$$$ mais fácil acessar isso direto na variável typeinfo como child da classe Action
            //        sum_wQi += secondaryload * t.phi0;
            //    }
            //    // G + Wk1 + Σ(φᵢ₀Qkᵢ)
            //    wW.Add(wGinst + (wW1 + sum_wQi));


            //    // Calculates frequent load combination (see EC0 Section 6.5.3)
            //    sum_wQi = new Displacement("Q");
            //    foreach (Displacement secondaryload in wQk)
            //    {
            //        TypeInfo t = new TypeInfo(secondaryload.type);
            //        sum_wQi += secondaryload * t.phi2;
            //    }
            //    // G + φᵢ₁Qk1 + Σ(φᵢ₂Qkᵢ)
            //    wW.Add(wGinst + (wW1 * t1.phi1 + sum_wQi));


            //    // Calculates quasi-permanent load combination (see EC0 Section 6.5.3)
            //    sum_wQi = new Displacement("Q");
            //    foreach (Displacement secondaryload in wQk)
            //    {
            //        TypeInfo t = new TypeInfo(secondaryload.type);
            //        sum_wQi += secondaryload * t.phi2;
            //    }
            //    // G + Σ(φᵢ₂Qkᵢ)
            //    wQ.Add(wGinst + (wW1 + sum_wQi));
            //}

        }
    }    
}
