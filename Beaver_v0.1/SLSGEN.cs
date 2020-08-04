using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Beaver_v0._1
{
    public class SLSGEN : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SLSGEN class.
        /// </summary>
        public SLSGEN()
          : base("SLS Analysis", "SLS Analysis",
              "Generates instantaneous and creep deflections for SLS analysis",
              "Beaver", "Combinations")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Caracteristic Deflectons", "Wk", "Caracteristics deflections for each load case", GH_ParamAccess.list);
            pManager.AddTextParameter("Actions Type", "type", "Action type according to EN1990: AnnexA1 table A1.1. Valid action type names: P (permanent), QA (domestic), QB (office), QC (congregation), QD (shopping), QE (storage), QG (traffic), QH (roof), QS (snow), W (wind)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Span Lenght", "span", "Span analysed", GH_ParamAccess.item);
            pManager.AddNumberParameter("Instantaneous Deflection Limit", "Winst,lim", "Ratio between the span lenght(l) and maximum instantaneous deflection (Winst) according to Table 7.2 in EN1995", GH_ParamAccess.item,350);
            pManager.AddNumberParameter("Net Final Deflection Span Limit", "Wnet,lim", "Ratio between the span lenght (l) and maximum net final deflection (Wnet,fin) according to Table 7.2 in EN1995", GH_ParamAccess.item,250);
            pManager.AddNumberParameter("Final Deflection Limit", "Wfin,lim", "Ratio between the span lenght (l) and maximum final deflection (Wfin) according to Table 7.2 in EN1995", GH_ParamAccess.item, 150);
            pManager.AddIntegerParameter("Service Class", "SC", "Service Class according to EN1995", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) //corrigir nomes do outputs
        {
            pManager.AddNumberParameter("Instantaneous deflection", "Winst", "Most relevant instantaneous deflection", GH_ParamAccess.list);
            pManager.AddNumberParameter("Net deflection", "Wnet", "Most relevant net final deflection", GH_ParamAccess.list);
            pManager.AddNumberParameter("Final deflection", "Wfin", "Most relevant final deflection", GH_ParamAccess.list);
            pManager.AddNumberParameter("Instantaneous Deflection ratio", "Winst/Winst,lim [%]", "Most relevant instantaneous deflection ratio", GH_ParamAccess.list);
            pManager.AddNumberParameter("Net ratio", "Wnet/Wnet,lim [%]", "Most relevant net deflection ratio", GH_ParamAccess.list);
            pManager.AddNumberParameter("Final ratio", "Wfin/Wfin,lim [%]", "Most relevant final deflection ratio", GH_ParamAccess.list);
            pManager.AddTextParameter("Combination Info", "Info", "Equation considered in combination", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //RETREIVE INPUTS
            List<double> wk = new List<double>();
            List<string> type = new List<string>();
            double span = 0;
            double Winstlim = 0;
            double Wnetlim = 0;
            double Wfinlim = 0;
            int SC = 0;
            DA.GetDataList(0, wk);
            DA.GetDataList(1, type);
            DA.GetData(2, ref span);
            DA.GetData(3, ref Winstlim);
            DA.GetData(4, ref Wnetlim);
            DA.GetData(5, ref Wfinlim);
            DA.GetData(6, ref SC);
            List<Action> wgk = new List<Action>();
            List<Action> wqk = new List<Action>();
            List<Action> wwk = new List<Action>();
            for (int i = 0; i < wk.Count; i++)
            {
                Action a = new Action(wk[i], type[i]);
                if (type[i].Contains("P"))
                {
                    wgk.Add(a);
                }
                else if (type[i].Contains("Q"))
                {
                    wqk.Add(a);
                }
                else if (type[i].Contains("W"))
                {
                    wwk.Add(a);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Type name does not match with any existing type (see EN1990)");
                }
            }
            
            //WINST DEFLECTION
            double wginst = 0;
            for (int i = 0; i < wgk.Count; i++)
            {
                wginst += wgk[i].Sk;
            }
            List<Action> Wq = new List<Action>();
            for (int i = 0; i < wqk.Count; i++)
            {
                List<Action> SQa = new List<Action>(wqk);
                SQa.RemoveAt(i);
                Action WQMain = wqk[i];
                double SumWi = 0;
                for (int j = 0; j < SQa.Count; j++)
                {
                    TypeInfo info = new TypeInfo(SQa[j].type);
                    SumWi += SQa[j].Sk * info.phi0;
                }
                Wq.Add(new Action(wginst + (WQMain.Sk + SumWi), WQMain.type));
                foreach (Action a in wwk)
                {
                    Wq.Add(new Action(wginst + (WQMain.Sk + SumWi + 0.6 * a.Sk), WQMain.type));
                }
            }
            double maxComb = 0;
            int idxmaxComb = 0;
            for (int i = 0; i < Wq.Count; i++)
            {
                TypeInfo info = new TypeInfo(Wq[i].type);
                double wcomb = Math.Abs(Wq[i].Sk);
                if (wcomb > maxComb)
                {
                    maxComb = wcomb;
                    idxmaxComb = i;
                }
            }
            double winst = Wq[idxmaxComb].Sk;
           
            //WNET,FIN DEFLECTION
            double wgnet = 0;
            for (int i = 0; i < wgk.Count; i++)
            {
                wgnet += wgk[i].Sk*(1+kdef(SC));
            }
            List<Action> Wqnet = new List<Action>();
            for (int i = 0; i < wqk.Count; i++)
            {
                List<Action> SQa = new List<Action>(wqk);
                SQa.RemoveAt(i);
                Action WQMain = wqk[i];
                double SumWc = 0;
                for (int j = 0; j < SQa.Count; j++)
                {
                    TypeInfo info = new TypeInfo(SQa[j].type);
                    SumWc += SQa[j].Sk * (info.phi0+kdef(SC)*info.phi2);
                }
                TypeInfo maininfo = new TypeInfo(WQMain.type);
                double wqmaincreep = WQMain.Sk * (1 + kdef(SC) * maininfo.phi2);
                Wqnet.Add(new Action(wgnet + wqmaincreep + SumWc, WQMain.type));
                foreach (Action a in wwk)
                {
                    Wqnet.Add(new Action(wgnet + (WQMain.Sk + SumWc + 0.6 * a.Sk), WQMain.type));
                }
            }
            double maxCombnet = 0;
            int idxmaxCombnet = 0;
            for (int i = 0; i < Wqnet.Count; i++)
            {
                TypeInfo info = new TypeInfo(Wq[i].type);
                double wcomb = Math.Abs(Wq[i].Sk);
                if (wcomb > maxComb)
                {
                    maxCombnet = wcomb;
                    idxmaxCombnet = i;
                }
            }

            //WNET,FIN DEFLECTION
            double wfin = 0;
            //falta calcular o wfin que é igual ao wnet mas subtraindo o valor da contra flecha wc


            double wnet = Wqnet[idxmaxCombnet].Sk;
            double winstlim = span / Winstlim;
            double wnetlim = span / Wnetlim;
            double wfinlim = span / Wfinlim;
            double instUtil = winst / winstlim;
            double netUtil = wnet / wnetlim;
            double finUtil = wfin / wfinlim; 
            List<string> Info = new List<string>();
            Info.Add("wG + w" + Wq[idxmaxComb].type + " + Σ(φᵢ₀wQᵢ)");
            Info.Add("(1 + kdef)(wG + Σ(φᵢ₂wQᵢ)");
            DA.SetData(0, winst);
            DA.SetData(1, wnet);
            DA.SetData(2, wfin);
            DA.SetData(3, instUtil);
            DA.SetData(4, netUtil);
            DA.SetData(5, finUtil);
            DA.SetData(6, Info);
        }
    
            double kdef(int SC) { 
            double k = 0;
            if (SC == 1)
            {
                k = 0.6;
            }
            else if (SC == 2)
            {
                k = 0.8;
            }
            else if (SC == 3)
            {
                k = 2;
            }
            return k;
        }
        class Action
        {
            public double Sk;
            public string type;
            public Action() { }
            public Action(double sk, string t)
            {
                Sk = sk;
                type = t;
            }
        }
        class TypeInfo
        {
            public double phi0;
            public double phi1;
            public double phi2;
            public string duration;

            public TypeInfo() { }
            public TypeInfo(string type)
            {
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
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources._1_2;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("792bd777-fe4c-4b42-b2b5-b712591d212d"); }
        }
    }
}