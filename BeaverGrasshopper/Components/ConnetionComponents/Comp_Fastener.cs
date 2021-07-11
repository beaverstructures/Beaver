using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using BeaverCore.Connections;
using System.Drawing;

namespace BeaverGrasshopper.Components.ConnetionComponents
{
    public class Comp_Fastener : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Comp_Fastener()
          : base("MyComponent1", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
        }


        public override void AddedToDocument(GH_Document document)
        {
            if (Params.Input[0].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate new value list
                var vl = new Grasshopper.Kernel.Special.GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "FType";
                //clear default contents
                vl.ListItems.Clear();
                vl.ListItems.Add(new GH_ValueListItem("Screw", "\"screw\""));
                vl.ListItems.Add(new GH_ValueListItem("Bolt", "\"bolt\""));
                vl.ListItems.Add(new GH_ValueListItem("Dowel", "\"dowel\""));
                vl.ListItems.Add(new GH_ValueListItem("Nail", "\"nail\""));
                document.AddObject(vl, false);
                Params.Input[0].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[0].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
            }
            base.AddedToDocument(document);
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Fastener Type", "Ftype", "Fastener Type", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "Fastener length [mm]", GH_ParamAccess.item, 8);
            pManager.AddNumberParameter("Diameter", "D", "Fastener nominal diameter [mm]", GH_ParamAccess.item, 8);
            pManager.AddNumberParameter("Shank Diameter", "Ds", "Fastener shank diameter [mm]", GH_ParamAccess.item, 8);
            pManager.AddNumberParameter("Head Diameter", "Dh", "Fastener head diameter [mm]", GH_ParamAccess.item, 12.5);
            pManager.AddNumberParameter("Fastener Fu", "Fu", "Fastener tensile ultimate strength [kN/cm²]", GH_ParamAccess.item, 360);
            pManager.AddBooleanParameter("Smooth Shank", "Smooth", "True for smooth nail shank, false for other.", GH_ParamAccess.item, true);
            Params.Input[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Fastener fastener = new Fastener();
            string Ftype = "none";
            DA.GetData(0, ref Ftype);

            if (Ftype == "screw")
            {
                double L = 0;
                double D = 0;
                double Ds = 0;
                double Dh = 0;
                double Fu = 0;
                DA.GetData(1, ref D);
                DA.GetData(2, ref Ds);
                DA.GetData(3, ref Dh);
                DA.GetData(4, ref L);
                DA.GetData(5, ref Fu);
                fastener = new Fastener(Ftype, D, Ds, Dh, Fu);
                DA.SetData(0, new GH_Fastener](fastener));
            }
            else if (Ftype == "bolt")
            {
                double L = 0;
                double D = 0;
                double Dh = 0;
                double Fu = 0;
                DA.GetData(1, ref D);
                DA.GetData(2, ref Dh);
                DA.GetData(3, ref L);
                DA.GetData(4, ref Fu);
                fastener = new Fastener(Ftype, D, Dh, L, Fu);
            }
            else if (Ftype == "dowel")
            {
                double L = 0;
                double D = 0;
                double Fu = 0;
                DA.GetData(1, ref D);
                DA.GetData(3, ref L);
                DA.GetData(4, ref Fu);
                fastener = new Fastener(Ftype, D, L, Fu);
            }
            else if (Ftype == "nail")
            {
                double L = 0;
                double D = 0;
                double Dh = 0;
                bool Smooth = true;
                double Fu = 0;
                DA.GetData(1, ref D);
                DA.GetData(2, ref Dh);
                DA.GetData(4, ref L);
                DA.GetData(5, ref Smooth);
                DA.GetData(5, ref Fu);
                fastener = new Fastener(Ftype, D, Dh, L, Smooth, Fu);
            }
            else
            {
                throw new ArgumentException("Something wrong with Ftype");
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC62C6D-7C03-412D-8FF8-B76439197730"); }
        }
    }
}