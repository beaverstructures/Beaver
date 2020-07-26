using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.Drawing;

namespace Beaver_v0._1
{
    public class C_MultipleInclined : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public C_MultipleInclined()
          : base("Multiple Axially Loaded Screws", "Multiple Axially Loaded",
              "Calculates the overall resistance of multiple axially loaded fastener connections considering the effective number of fasteners (nef)",
              "Beaver", "Connections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Design Shear Force [N]", "Vrd", "The design shear force for the connection", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Multiple configuration type", "Type", "Type of multiple connection", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of screw pairs", "Npair", "The number of paired connections", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Load and screw direction angle [rad]", "β", "Angle between the force and screw direction",GH_ParamAccess.item, Math.PI / 4);
            pManager.AddNumberParameter("Tension resistance [N]", "Ftrd", "Tensioned axially loaded screw load capacity", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Compression resistance [N]", "Fcrd", "Compressed axially loaded screw load capacity", GH_ParamAccess.item, 0);
        
    }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Shear Strenght", "Rcvd", "Connection Design Load Carrying Capacity [N]");
            pManager.Register_DoubleParam("Util", "Util", "Reason between Stress and Strength");
            pManager.Register_DoubleParam("Effective Number of Fasteners", "Nef", "Effective Number of Fasteners");
        }

        public override void AddedToDocument(GH_Document document)
        {
            if (Params.Input[1].SourceCount == 0)
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
                var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Crossed", "0");
                var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Parallel Tension", "1");
                var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("Parallel Compression", "2");
                vl.ListItems.Add(item1); vl.ListItems.Add(item2); vl.ListItems.Add(item3);

                document.AddObject(vl, false);
                Params.Input[1].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[1].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 180, currPivot.Y - 11);
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
            int type = 0;
            int npair = 0;
            double beta = 0;
            double FTrd = 0;
            double FCrd = 0;

            if (!DA.GetData<double>(0, ref Vrd)) { return; }
            if (!DA.GetData<int>(1, ref type)) { return; }
            if (!DA.GetData<int>(2, ref npair)) { return; }
            if (!DA.GetData<double>(3, ref beta)) { return; }
            if (!DA.GetData<double>(4, ref FTrd)) { return; }
            if (!DA.GetData<double>(5, ref FCrd)) { return; }
            double n = 2 * npair;
            double Fvd = 0;
            double nef = 0;
            if (type == 0)
            {
                nef = Math.Pow(npair, 0.9) * 2;
                Fvd = nef * (FTrd + FCrd) * Math.Cos(beta) / 2;
            }
            else
            {
                nef = Math.Pow(n, 0.9);
                if (type == 1)
                {
                    Fvd = nef * FTrd * Math.Cos(beta);
                }
                else if (type == 2)
                {
                    Fvd = nef * FCrd * Math.Cos(beta);
                }
            }
            double Util = Vrd / Fvd;
            DA.SetData(0, Fvd);
            DA.SetData(1, Util);
            DA.SetData(2, nef);
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
                return Properties.Resources._3_9;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("28efbec7-864f-442a-b25a-63a7e5e9ef66"); }
        }
    }
}