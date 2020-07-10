﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.IO;
using System.Drawing;

namespace Beaver_v0._1
{
    public class C_ScrewT2T : GH_Component
    {
        GH_Document GrasshopperDocument;
        IGH_Component Component;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public C_ScrewT2T()
          : base("Screw - Timber to Timber", "Screw T2T",
              "Analysis of ductile failure of screwed timber-to-timber connections according to Eurocode 5.",
              "Beaver", "Connections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddNumberParameter("Timber depth 1", "t1", "Timber depth 1 [mm]", GH_ParamAccess.item, 60);
            pManager.AddNumberParameter("Timber depth 2", "t2", "Timber depth 2 [mm]", GH_ParamAccess.item, 40);
            pManager.AddNumberParameter("Alpha1", "α1", "Force Angle related to the grain of t1", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Alpha2", "α2", "Force Angle related to the grain of t2", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Alpha Screw", "αs", "Screw angle related to the grain of t [rad]", GH_ParamAccess.item, 1.57);
            pManager.AddNumberParameter("Diameter", "d", "Shank Diameter", GH_ParamAccess.item, 8);
            pManager.AddNumberParameter("Diameter", "dh", "Head Diameter", GH_ParamAccess.item, 8);
            pManager.AddNumberParameter("Lenght", "l", "Screw Length", GH_ParamAccess.item, 80);
            pManager.AddNumberParameter("Threaded lenght", "l_th", "Screw Threaded lenght", GH_ParamAccess.item, 40);
            pManager.AddTextParameter("WoodType", "wtype", "", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Pre Drilled", "pdrill", "Pre-drilled boolean", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Single or Double Shear", "St", "0 for Single Shear, 1 for Double", GH_ParamAccess.item, 0);
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
            pManager.Register_StringParam("Failure Mode", "Fail. Mode", "Failure mode for calculated Load Carrying Capacity");
        }

        public override void AddedToDocument(GH_Document document)
        {
            Material timber = new Material();
            List<string> names = timber.GetTypesNames();
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
                foreach (string name in names)
                {
                    vl.ListItems.Add(new GH_ValueListItem(name, "\"" + name + "\""));
                }

                document.AddObject(vl, false);
                Params.Input[9].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[9].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
            }
            if (Params.Input[11].SourceCount == 0)
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
                var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Double Shear", "1");
                vl.ListItems.Add(item1); vl.ListItems.Add(item2);

                document.AddObject(vl, false);
                Params.Input[11].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[11].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 180, currPivot.Y - 11);
            }
            base.AddedToDocument(document);
        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double t1 = 0;
            double t2 = 0;
            double al1 = 0;
            double al2 = 0;
            double alfast = 0;
            double a1 = 500;
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
            double sd = 0;
            double kmod = 0;
            double Nrd = 0;
            double Vrd = 0;
            double Ym = 0;
            double fsteel = 0;


            if (!DA.GetData<double>(0, ref t1)) { return; }
            if (!DA.GetData<double>(1, ref t2)) { return; }
            if (!DA.GetData<double>(2, ref al1)) { return; }
            if (!DA.GetData<double>(3, ref al2)) { return; }
            if (!DA.GetData<double>(4, ref alfast)) { return; }
            if (!DA.GetData<double>(5, ref d)) { return; }
            if (!DA.GetData<double>(6, ref dh)) { return; }
            if (!DA.GetData<double>(7, ref l)) { return; }
            if (!DA.GetData<double>(8, ref lt)) { return; }
            if (!DA.GetData<string>(9, ref wood)) { return; }
            if (!DA.GetData<bool>(10, ref pdrill)) { return; }
            if (!DA.GetData<double>(11, ref sd)) { return; }
            if (!DA.GetData<double>(12, ref kmod)) { return; }
            if (!DA.GetData<double>(13, ref fsteel)) { return; }


            Material timber = new Material(wood);
            pk = timber.pk;
            string woodtype = timber.name;
            Ym = 1.3;
            //CALCULO DAS LIGAÇÕESS
            Ccalc_Fastener fast = new Ccalc_Fastener(type, d, dh, l, true, fsteel);
            Ccalc_T2TCapacity analysis = new Ccalc_T2TCapacity(fast, t1, t2, al1, al2, woodtype, "steel", pdrill, pk, pk, dh, woodtype, lt, alfast, npar, npep, a1);
            double fvd = 0;
            string failureMode = "";
            if (sd == 0)
            {
                dynamic cap = analysis.FvkSingleShear();
                fvd = kmod * cap.Fvrk / Ym;
                failureMode = cap.failureMode;
            }
            else
            {
                dynamic cap = analysis.FvkDoubleShear();
                fvd = kmod * cap.Fvrk / Ym;
                failureMode = cap.failureMode;
            }
            double faxd = kmod * analysis.variables.Faxrk / Ym;
            double DIV = 0;
            if (fast.smooth == true)
            {
                DIV =  Nrd / faxd +  Vrd / fvd;
            }
            else
            {
                DIV = Math.Pow(Nrd / faxd, 2) + Math.Pow(Vrd / fvd, 2);
            }
            DA.SetData(0, fvd);
            DA.SetData(1, faxd);
            DA.SetData(2, failureMode);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return Properties.Resources._3_1;
                // return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6f1615b5-3f3d-40c9-9eef-f4549c52b7ea"); }
        }
    }
}