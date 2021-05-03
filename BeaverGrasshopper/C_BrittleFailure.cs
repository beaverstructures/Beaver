using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.IO;
using System.Drawing;

namespace Beaver_v0._1
{
    public class C_BrittleFailure : GH_Component
    {
        GH_Document GrasshopperDocument;
        IGH_Component Component;
        /// <summary>
        /// Initializes a new instance of the verBFcs class.
        /// </summary>
        public C_BrittleFailure()
          : base("Brittle Failure", "Brittle Failure",
              "Verifies Minimum Fastener Distances to prevent Brittle Failure",
              "Beaver", "Connections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Fastener Type", "FType", "Fastener Type: input nail, screw, bolt or dowel as text", GH_ParamAccess.item, "screw");
            pManager.AddNumberParameter("Diameter", "d", "Fastener Diameter, in the case os screws equivalent to shank diameter [mm]", GH_ParamAccess.item, 8);
            pManager.AddTextParameter("WoodType", "wtype", "", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Force Alpha", "α", "Force Angle related to the grain", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Pre Drilled", "pdrill", "", GH_ParamAccess.item, false);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("a1", "a1", "Minimum paralel to the grain spacing between fasteners");
            pManager.Register_DoubleParam("a1best", "a1best", "Best paralel to the grain spacing between fasteners "+
                "(Above this condition the effective number of fasteners will be equal to the real number of fasteners on multiple fastener connections)");
            pManager.Register_DoubleParam("a2", "a2", "Minimum Perpendicular to the Grain Spacing Between Fasteners");
            pManager.Register_DoubleParam("a3c", "a3c", "Minimum paralel to the grain spacing Between last fastener and the cutted edge of the timber element " +
                "(if alpha points towards the cutted edge use a3t, if alpha does not points towards the cutted edge use a3c)");
            pManager.Register_DoubleParam("a3t", "a3t", "Minimum paralel to the grain spacing Between last Fastener and the cutted edge of the timber element" +
                "(if alpha points towards the cutted edge use a3t, if alpha does not points towards the cutted edge use a3c)");
            pManager.Register_DoubleParam("a4c", "a4c", "Minimum perpendicular to the grain spacing between last fastener and the lower/upper edge of the timber element" +
                "(alpha points towards the a4t lowwer/upper edge, while it does not poits towards the lower/upper a4c edge)");
            pManager.Register_DoubleParam("a4t", "a4t", "Minimum perpendicular to the grain spacing between Last fastener and the cutted edge of the timber element" +
                "(alpha points towards the a4t lowwer/upper edge, while it does not poits towards the lower/upper a4c edge)");

        }

        public override void AddedToDocument(GH_Document document)
        {
            Material timber = new Material();
            List<string> names = timber.GetTypesNames();
            if (Params.Input[2].SourceCount == 0)
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
                Params.Input[2].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[2].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
            }
        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
           
            string type = "screw";
            double d = 0;
            double pk = 0;
            double alfa = 0;
            bool pdrill = false;
            string wood = "";



            if (!DA.GetData<string>(0, ref type)) { return; }
            if (!DA.GetData<double>(1, ref d)) { return; }
            if (!DA.GetData<string>(2, ref wood)) { return; }
            if (!DA.GetData<double>(3, ref alfa)) { return; }
            if (!DA.GetData<bool>(4, ref pdrill)) { return; }
            if (type == "dowel") { type = "bolt"; }

            Material timber = new Material(wood);
            string woodtype = timber.name;
            pk = timber.pk;
            
            //CALCULOS DE BRITTLE FAILURE (minimos espaçamentos aceitáveis)
            Ccalc_Fastener fast = new Ccalc_Fastener(type, d, -1, -1, true, -1);
            Ccalc_BrittleFailure analysis = new Ccalc_BrittleFailure(fast, pk, alfa, pdrill);
            double a1 = analysis.a1;
            double a2 = analysis.a2;
            double a3c = analysis.a3c;
            double a3t = analysis.a3t;
            double a4c = analysis.a4c;
            double a4t = analysis.a4t;
            double a1best = 14 * d;

            DA.SetData(0, a1);
            DA.SetData(1, a1best);
            DA.SetData(2, a2);
            DA.SetData(3, a3c);
            DA.SetData(4, a3t);
            DA.SetData(5, a4c);
            DA.SetData(6, a4t);

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
                return Properties.Resources._3_8;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b7698978-409b-4474-a1f2-00fac6c092e7"); }
        }
    }
}