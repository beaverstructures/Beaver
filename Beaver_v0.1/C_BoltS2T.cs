using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.IO;
using System.Drawing;

namespace Beaver_v0._1
{
    public class C_BoltS2T : GH_Component
    {
        GH_Document GrasshopperDocument;
        IGH_Component Component;
        /// <summary>
        /// Initializes a new instance of the BoltS2T class.
        /// </summary>
        public C_BoltS2T()
          : base("Bolt or Dowel - Steel to Timber", "Bolt S2T",
              "Analysis of ductile failure of bolted or doweled steel-to-timber connections according to Eurocode 5",
              "Beaver", "Connections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Timber Depth", "t", "Timber Depth [mm]", GH_ParamAccess.item, 60); //0
            pManager.AddNumberParameter("Steel Depth", "tsteel", "Steel Depth [mm]", GH_ParamAccess.item, 6.3); //1
            pManager.AddNumberParameter("Alpha", "α", "Force Angle related to the grain of t [rad]", GH_ParamAccess.item, 0); //2
            pManager.AddNumberParameter("Fastener Diameter", "d", "Fastener Diameter [mm]", GH_ParamAccess.item, 8); 
            pManager.AddNumberParameter("Washer Diameter", "dw", "Washer's diameter [mm] (for dowels dw = d)", GH_ParamAccess.item, 8); 
            pManager.AddTextParameter("WoodType", "wtype", "", GH_ParamAccess.item, ""); //5
            pManager.AddIntegerParameter("Shear Type", "St", " 0 for Single Shear, 1 for Double with steel center member, 2 for Double with timber center member", GH_ParamAccess.item, 0); //6
            pManager.AddNumberParameter("Modification Factor", "Kmod", "Modification Factor for Load Duration and Moisture Content", GH_ParamAccess.item, 0.6);
            pManager.AddNumberParameter("Fastener fyk", "fyk", "Characteristic Yield Strength of the Fastener's steel [N/mm²]", GH_ParamAccess.item, 260);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Caracteristic Shear Strenght", "Fvrd", "Fastener Load Carrying Capacity per Shear Plane");
            pManager.Register_DoubleParam("Caracteristic Withdrawal capacity", "Faxrd", "Fastener Withdrawal Capacity considered");
            pManager.Register_StringParam("Failure Mode", "Fail.Mode", "Failure mode for calculated Load Carrying Capacity");
        }

        public override void AddedToDocument(GH_Document document)
        {
            Material timber = new Material();
            List<string> names = timber.GetTypesNames();
            if (Params.Input[5].SourceCount == 0)
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
                Params.Input[5].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[5].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
            }
            if (Params.Input[6].SourceCount == 0)
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
                Params.Input[6].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[6].Attributes.Pivot;
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
            double Vrd = 0;
            double t = 0;
            double tsteel = 0;
            double alfa = 0;
            double n_par = 1;
            double n_pep = 1;
            double d = 0;
            double dw = 0;
            string wood = "";
            int shear_type = 0;
            double kmod = 0;
            double a4 = 0;
            double Ym = 0;

            string woodtype = "";
            double fc90 = 0;

            //Constant Values
            string type = "bolt";
            bool pre_drilled = true;
            double pk = 0;
            double l = -1; //irrelevant
            bool smooth = true; //irrelevant
            double fsteel = 0;
            double alfa_fast = -1;//irrelevant

            if (!DA.GetData<double>(0, ref t)) { return; }
            if (!DA.GetData<double>(1, ref tsteel)) { return; }
            if (!DA.GetData<double>(2, ref alfa)) { return; }
            if (!DA.GetData<double>(3, ref d)) { return; }
            if (!DA.GetData<double>(4, ref dw)) { return; }
            if (!DA.GetData<string>(5, ref wood)) { return; }
            if (!DA.GetData<int>(6, ref shear_type)) { return; }
            if (!DA.GetData<double>(7, ref kmod)) { return; }
            if (!DA.GetData<double>(8, ref fsteel)) { return; }
            double fu = fsteel; //default

            Material timber = new Material(wood);
            pk = timber.pk;
            fc90 = 10 * timber.fc90k;
            Ym = 1.3;
            woodtype = timber.name;

            //CALCULO DAS LIGAÇÕES
            var fast = new Ccalc_Fastener(type, d, dw, l, smooth, fu);
            var analysis = new Ccalc_T2SCapacity(fast, pre_drilled, pk, alfa, alfa_fast, woodtype, t, tsteel, fc90, n_par, n_pep, shear_type, 500);
            double fvd = 0;
            string failureMode = "";
            if (shear_type == 0)
            {
                dynamic cap = analysis.FvrkSingleShear();
                fvd = kmod * cap.Fvrk / Ym;
                failureMode = cap.failureMode;
            }
            else
            {
                dynamic cap = analysis.FvrkDoubleShear();
                fvd = kmod * cap.Fvrk / Ym;
                failureMode = cap.failureMode;
            }
            double faxd = kmod * analysis.variables.Faxrk / Ym;



            //Steel Plate
            double Fsrd = Math.Min(1.2 * a4 * tsteel * 400 / 1.35, 2.4 * fast.d * tsteel * 400 / 1.35);
            double Utilsteel = Vrd / Fsrd;

            DA.SetData(0, fvd);
            DA.SetData(1, faxd);
            DA.SetData(2, failureMode);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources._3_5;
                // return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("720ebda3-2e25-4560-a4e7-b55a6392d207"); }
        }
    }
}