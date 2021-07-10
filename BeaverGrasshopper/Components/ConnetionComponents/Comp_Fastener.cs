using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using BeaverCore.Connections;

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

            string Ftype = "none";
            double L = 0;
            double D = 0;
            double Ds = 0;
            double Dh = 0;
            double Fu = 0;
            bool Smooth = true;

            DA.GetData(0, ref Ftype);
            DA.GetData(1, ref L);
            DA.GetData(2, ref D);
            DA.GetData(3, ref Ds);
            DA.GetData(4, ref Dh);
            DA.GetData(5, ref Fu);
            DA.GetData(6, ref Smooth);

            Fastener Fastener = new Fastener();
            if (Ftype == "dowel") { Fastener = new Fastener(Ftype, D, L, Fu); }
            else if (Ftype == "bolt") { Fastener = new Fastener(Ftype, D, Dh, L, Fu); }
            else if (Ftype == "nail") { Fastener = new Fastener(Ftype, D, Dh,L, Smooth, Fu); }
            else if (Ftype == "screw") { Fastener = new Fastener(Ftype, D, Ds, Dh, Fu); }


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