using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Beaver_v0._1
{
    public class CombGen : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CombGen()
          : base("ULS Combination Generator", "ULS Combinations",
              "Generates relevant results combinations for ULS analysis",
              "Beaver", "Combinations")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Caracteristic Actions", "Ek", "Caracteristics values for each load case", GH_ParamAccess.list);
            pManager.AddTextParameter("Actions Type", "type", "Action type according to EN1990: AnnexA1 table A1.1. Valid action type names: P (permanent), QA (domestic), QB (office), QC (congregation), QD (shopping), QE (storage), QG (traffic), QH (roof), QS (snow), W (wind)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Service Class", "SC", "Service Class according to EN1995", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Design Actions", "Ed", "Design values of all combinations", GH_ParamAccess.list);
            pManager.AddNumberParameter("Modification Factor", "Kmods", "Modification factor associated with the combination", GH_ParamAccess.list);
            pManager.AddTextParameter("Combination Info", "Info", "Equation considered in combination", GH_ParamAccess.list);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //RETREIVE INPUTS
            List<double> Sk = new List<double>();
            List<string> type = new List<string>();
            int SC = 0;
            DA.GetDataList(0, Sk);
            DA.GetDataList(1, type);
            DA.GetData(2, ref SC);
            if (Sk.Count != type.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The number of caracteristic actions is not equal to action types");
            }
            //DEFINE PARAMETERS
            List<Action> SGk = new List<Action>();
            List<Action> SQk = new List<Action>();
            List<Action> SWk = new List<Action>();
            List<double> Combinations = new List<double>();
            List<double> Kmods = new List<double>();
            List<string> Out = new List<string>();
            for (int i = 0; i< Sk.Count; i++) {
                Action a = new Action(Sk[i], type[i]);
                if (type[i].Contains("P"))
                {
                    SGk.Add(a);
                }
                else if (type[i].Contains("Q"))
                {
                    SQk.Add(a);
                }
                else if (type[i].Contains("W"))
                {
                    SWk.Add(a);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Type number x does not match with any existing type (see EN1990)");
                }
            }
            //PERMANENT LOADING
            double P = 0;
            foreach (Action act in SGk)
            {
                P += act.Sk;
            }
            Combinations.Add(1.35*P);
            Kmods.Add(KMOD(SC,"perm"));
            Out.Add("1.35G");
            //ACIDENTAL LOADING
            List<Action> Comb = new List<Action>();
            for (int i=0; i < SQk.Count; i++)
            {
                List<Action> SQa = new List<Action>(SQk);
                SQa.RemoveAt(i);
                Action Qmain = SQk[i];
                double SumQi = 0;
                for (int j = 0; j < SQa.Count; j++)
                {
                    TypeInfo info = new TypeInfo(SQa[j].type);
                    SumQi += SQa[j].Sk * info.phi0;
                }
                Comb.Add(new Action(1.35 * P + 1.5 * (Qmain.Sk + SumQi), Qmain.type));
                foreach (Action a in SWk)
                {
                    Comb.Add(new Action(1.35 * P + 1.5 * (Qmain.Sk + SumQi+0.6*a.Sk), Qmain.type));
                }
            }
            double maxComb = 0;
            int idxmaxComb = 0;
            double minComb = 0;
            int idxminComb = 0;
            for (int i = 0; i < Comb.Count; i++)
            {
                TypeInfo info = new TypeInfo(Comb[i].type);
                double wcomb = Comb[i].Sk / KMOD(SC, info.duration);
                if (wcomb > maxComb)
                {
                    maxComb = wcomb;
                    idxmaxComb = i;
                }
                if (wcomb < minComb)
                {
                    minComb = wcomb;
                    idxminComb = i;
                }
            }
            if (maxComb > 0)
            {
                Combinations.Add(Comb[idxmaxComb].Sk);
                Kmods.Add(KMOD(SC, (new TypeInfo(Comb[idxmaxComb].type)).duration));
                Out.Add("1.35G + 1.5 " + Comb[idxmaxComb].type + " + 1.5Σ(φᵢ₀Qᵢ)");
            }
            if (minComb < 0)
            {
                Combinations.Add(Comb[idxminComb].Sk);
                Kmods.Add(KMOD(SC, (new TypeInfo(Comb[idxminComb].type)).duration));
                Out.Add("1.35G + 1.5 " + Comb[idxminComb].type + " + 1.5Σ(φᵢ₀Qᵢ)");
            }
            //WIND LOADING
            List<Action> WComb = new List<Action>();
            double SumQ = 0;
            foreach (Action a in SQk)
            {
                TypeInfo info = new TypeInfo(a.type);
                SumQ += a.Sk * info.phi0;
            }
            maxComb = 0;
            idxmaxComb = 0;
            minComb = 0;
            idxminComb = 0;
            for (int i = 0; i < SWk.Count; i++)
            {
                WComb.Add(new Action(1.35 * P + 1.5 * (SWk[i].Sk + SumQ),"W"));
                WComb.Add(new Action(P + 1.5 * (SWk[i].Sk), "W"));
            }
            for (int i = 0; i < WComb.Count; i++)
            {
                TypeInfo info = new TypeInfo(WComb[i].type);
                double wcomb = WComb[i].Sk / KMOD(SC, info.duration);
                if (wcomb > maxComb)
                {
                    maxComb = wcomb;
                    idxmaxComb = i;
                }
                if (wcomb < minComb)
                {
                    minComb = wcomb;
                    idxminComb = i;
                }
            }
            if (maxComb > 0)
            {
                Combinations.Add(WComb[idxmaxComb].Sk);
                Kmods.Add(KMOD(SC, (new TypeInfo(WComb[idxmaxComb].type)).duration));
                if (idxmaxComb % 2 == 0)
                {
                    Out.Add("1.35G + 1.5W + 1.5Σ(φᵢ₀Qᵢ)");
                }
                else
                {
                    Out.Add("G + 1.5W");
                }
            }
            if (minComb<0) 
            {
                Combinations.Add(WComb[idxminComb].Sk);
                Kmods.Add(KMOD(SC, (new TypeInfo(WComb[idxminComb].type)).duration));
                if (idxminComb % 2 == 0)
                {
                    Out.Add("1.35G + 1.5W + 1.5Σ(φᵢ₀Qᵢ)");
                }
                else
                {
                    Out.Add("G + 1.5W");
                }
            }
            
            DA.SetDataList(0, Combinations);
            DA.SetDataList(1, Kmods);
            DA.SetDataList(2, Out);

        }
        class Action
        {
            public double Sk;
            public string type;
            public Action() {}
            public Action (double sk, string t)
            {
                Sk = sk;
                type = t;
            }
        }
        public double KMOD (int SC,string duration)
        {
            double k = 0;
            if (SC == 1 || SC == 2)
            {
                if (duration == "perm")
                {
                    k = 0.6;
                }
                else if (duration == "long")
                {
                    k = 0.7;
                }
                else if (duration == "medium")
                {
                    k = 0.8;
                }
                else if (duration == "short")
                {
                    k = 0.9;
                }
                else if (duration == "inst")
                {
                    k = 1.1;
                }
            }
            else if (SC == 3)
            {
                if (duration == "perm")
                {
                    k = 0.5;
                }
                else if (duration == "long")
                {
                    k = 0.55;
                }
                else if (duration == "medium")
                {
                    k = 0.65;
                }
                else if (duration == "short")
                {
                    k = 0.7;
                }
                else if (duration == "inst")
                {
                    k = 0.9;
                }
            }
            else { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Service Class must be a integer between 1 and 3"); }
            return k;
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
                return Properties.Resources._1_1;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0cb6c387-f282-4e23-83b4-3bafc15688ca"); }
        }
    }
}