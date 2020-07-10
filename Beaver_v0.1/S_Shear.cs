using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.IO;
using System.Drawing;

namespace Beaver_v0._1
{
    public class S_Shear : GH_Component
    {
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        public S_Shear()
          : base("Shear Forces", "Shear",
             "Verifies the cross section of a Beam subjected to Shear forces according to Eurocode 5",
              "Beaver", "Sections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddNumberParameter("Shear Force", "V", "Shear Force [kN]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Heigth", "h", "Section Heigth [cm]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Base", "b", "Section Base [cm]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Modification Factor", "Kmod", "Modification Factorbfor Duration of Load andd Moisture Content", GH_ParamAccess.item, 0.6);
            pManager.AddTextParameter("Material", "Material", "Section Material", GH_ParamAccess.item, "");
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("DIV", "DIV", "Reason between Stress and Strength");

        }

        public override void AddedToDocument(GH_Document document)
        {
            Material timber = new Material();
            List<string> names = timber.GetTypesNames();
            if (Params.Input[0].SourceCount == 0)
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
                Params.Input[4].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[4].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 200, currPivot.Y - 11);
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
            double V = 0;
            double h = 0;
            double b = 0;
            double Kmod = 0;
            double Gamm = 0;
            double Fvk = 0;
            string test = "";
            if (!DA.GetData<double>(0, ref V)) { return; }
            if (!DA.GetData<double>(1, ref h)) { return; }
            if (!DA.GetData<double>(2, ref b)) { return; }
            if (!DA.GetData<double>(3, ref Kmod)) { return; }
            if (!DA.GetData(4, ref test)) { return; }
            Material timber = new Material(test);
            Fvk = timber.fvk;
            Gamm = timber.Ym;
            double bef = 0.67 * b;
            double Sigv = (3 / 2) * (V / (bef * h));
            double fvd = Kmod * Fvk / Gamm;
            double Div = Sigv / fvd;
            DA.SetData(0, Div);
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
                return Properties.Resources._2_2;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("24a03e02-9505-44de-aca2-1d4b244a89a2"); }
        }
    }
}