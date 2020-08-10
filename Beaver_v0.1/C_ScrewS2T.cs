using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.IO;
using System.Drawing;

namespace Beaver_v0._1
{
    public class C_ScrewS2T : GH_Component
    {
        GH_Document GrasshopperDocument;
        IGH_Component Component;
        /// <summary>
        /// Initializes a new instance of the ScrewS2T class.
        /// </summary>
        public C_ScrewS2T()
          : base("Screw - Steel to Timber", "Screw S2T",
              "Analysis of ductile failure of screwed steel-to-timber connections according to Eurocode 5.",
              "Beaver", "Connections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Timber depth", "t", "Timber depth [mm]", GH_ParamAccess.item, 60); //0
            pManager.AddNumberParameter("Steel depth", "tsteel", "Steel depth [mm]", GH_ParamAccess.item, 6.3); //1
            pManager.AddNumberParameter("Alpha", "α", "Force Angle related to the grain of t [rad]", GH_ParamAccess.item, 0); //2
            pManager.AddNumberParameter("Diameter", "d", "Shank Diameter [mm]", GH_ParamAccess.item, 8);  //3
            pManager.AddNumberParameter("Diameter", "dh", "Head Diameter [mm]", GH_ParamAccess.item, 8); //4
            pManager.AddNumberParameter("Lenght", "l", "Screw Length [mm]", GH_ParamAccess.item, 80); //5
            pManager.AddNumberParameter("Threaded lenght", "l_th", "Screw Threaded lenght [mm]", GH_ParamAccess.item, 40); //6
            pManager.AddTextParameter("WoodType", "wtype", "", GH_ParamAccess.item, ""); //7
            pManager.AddBooleanParameter("Pre Drilled", "pdrill", "Pre-drilled boolean", GH_ParamAccess.item, false); //8
            pManager.AddIntegerParameter("Shear Type", "St", "0 for Single Shear, 1 for Double with steel center member, 2 for Double with timber center member", GH_ParamAccess.item, 0); //9
            pManager.AddNumberParameter("Modification Factor", "Kmod", "Modification Factor for Load Duration and Moisture Content", GH_ParamAccess.item, 0.6); //10
            pManager.AddNumberParameter("Fastener fyk", "fyk", "Characteristic Yield Strength of the Fastener's steel [N/mm²]", GH_ParamAccess.item, 260); //11
            pManager.AddNumberParameter("Alpha Screw", "αs", "Screw angle related to the grain [rad]", GH_ParamAccess.item, 1.57);
            pManager.AddBooleanParameter("Output Type", "Out", "true for retrieve the worst failure mode, false to retrieve all failure modes", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Caracteristic Shear Strenght", "Fvrd", "Fastener Load Carrying Capacity per Shear Plane");
            pManager.Register_DoubleParam("Caracteristic Withdrawal capacity", "Faxrd", "Fastener Withdrawal Capacity considered");
            pManager.Register_StringParam("Failure Mode", "Fail. Mode", "Failure mode for calculated Load Carrying Capacity");
            pManager.Register_StringParam("Parameter info", "Info", "Parameters data");
        }

        public override void AddedToDocument(GH_Document document)
        {
            Material timber = new Material();
            List<string> names = timber.GetTypesNames();
            if (Params.Input[7].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate new value list
                var vl = new Grasshopper.Kernel.Special.GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "Type";
                //clear default contents
                vl.ListItems.Clear();
                foreach (string name in names)
                {
                    vl.ListItems.Add(new GH_ValueListItem(name, "\"" + name + "\""));
                }

                document.AddObject(vl, false);
                Params.Input[7].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[7].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
            }
            if (Params.Input[9].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate new value list
                var vl = new Grasshopper.Kernel.Special.GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "Type";
                //clear default contents
                vl.ListItems.Clear();
                var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Single Shear", "0");
                var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Double Shear Steel In", "1");
                var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("Double Shear Steel Out", "2");
                vl.ListItems.Add(item1); vl.ListItems.Add(item2); vl.ListItems.Add(item3);

                document.AddObject(vl, false);
                Params.Input[9].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[9].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 225, currPivot.Y - 11);
            }
            base.AddedToDocument(document);
        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double t = 0;
            double tsteel = 0;
            double al = 0;
            double alfast = 0;
            double npar = 1;
            double npep = 1;
            double d = 0;
            double dh = 0;
            double l = 0;
            double lt = 0;
            string type = "screw";
            string wood = "";
            bool pdrill = false;
            double pk = 0;
            int sd = 0;
            double kmod = 0;
            double Ym = 0;
            double fsteel = 0;
            bool output = false;
            if (!DA.GetData<double>(0, ref t)) { return; }
            if (!DA.GetData<double>(1, ref tsteel)) { return; }
            if (!DA.GetData<double>(3, ref d)) { return; }
            if (!DA.GetData<double>(4, ref dh)) { return; }
            if (!DA.GetData<double>(5, ref l)) { return; }
            if (!DA.GetData<double>(6, ref lt)) { return; }
            if (!DA.GetData<string>(7, ref wood)) { return; }
            if (!DA.GetData<bool>(8, ref pdrill)) { return; }
            if (!DA.GetData<int>(9, ref sd)) { return; }
            if (!DA.GetData<double>(10, ref kmod)) { return; }
            if (!DA.GetData<double>(11, ref fsteel)) { return; }
            if (!DA.GetData<double>(12, ref alfast)) { return; }
            if (!DA.GetData<bool>(13, ref output)) { return; }
            Material timber = new Material(wood);
            pk = timber.pk;
            string woodtype = timber.name;
            Ym = 1.3;
            //CALCULO DAS LIGAÇÕESS
            Ccalc_Fastener fast = new Ccalc_Fastener(type, d, dh, l, true, fsteel);
            var analysis = new Ccalc_T2SCapacity(fast, pdrill, pk, al, alfast, woodtype, t, tsteel, lt, npar, npep, sd, 500);
            if (output)
            {
                double fvd = 0;
                string failureMode = "";
                if (sd == 0)
                {
                    dynamic cap = analysis.FvrkSingleShear(output);
                    fvd = kmod * cap.Fvrk / Ym;
                    failureMode = cap.failureMode;
                }
                else
                {
                    dynamic cap = analysis.FvrkDoubleShear(output);
                    fvd = kmod * cap.Fvrk / Ym;
                    failureMode = cap.failureMode;
                }
                DA.SetData(2, failureMode);
                DA.SetData(0, fvd);
            }
            else
            {
                List<double> fvd = new List<double>();
                List<string> failureMode = new List<string>();
                if (sd == 0)
                {
                    dynamic cap = analysis.FvrkSingleShear(output);
                    fvd = cap.Fvrks;
                    for (int i = 0; i < fvd.Count; i++)
                    {
                        fvd[i] = kmod * fvd[i] / Ym;
                    }
                    failureMode = cap.failures;
                }
                else
                {
                    dynamic cap = analysis.FvrkDoubleShear(output);
                    fvd = cap.Fvrks;
                    for (int i = 0; i < fvd.Count; i++)
                    {
                        fvd[i] = kmod * fvd[i] / Ym;
                    }
                    failureMode = cap.failures;
                }
                DA.SetDataList(2, failureMode);
                DA.SetDataList(0, fvd);
            }


            double faxd = kmod * analysis.variables.Faxrk / Ym;

            string info = string.Format("fhk: {0}, Mryk: {1}", Math.Round(analysis.variables.fhk, 3), Math.Round(analysis.variables.Myrk));
            DA.SetData(3, info);


            DA.SetData(1, faxd);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources._3_2;
                // return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d38fbee7-bf7d-45fc-9449-f85ab05367e3"); }
        }
    }
}