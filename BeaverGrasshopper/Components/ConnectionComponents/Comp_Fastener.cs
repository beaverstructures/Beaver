using BeaverCore.Actions;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using BeaverCore.Connections;

namespace BeaverGrasshopper
{
    public class Comp_Fastener : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Comp_Force class.
        /// </summary>
        public Comp_Fastener()
          : base("Comp_Fastener", "Nickname",
              "Description",
              "Beaver", "1.Frame")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("FastenerType", "Ftype", "Fastener type", GH_ParamAccess.item);
            pManager.AddNumberParameter("Nominal Diameter", "D", "Fastener nominal diamater", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Shank Diameter", "Ds", "Fastener shank diameter", GH_ParamAccess.item, 0); //screws
            pManager.AddNumberParameter("Head Diameter", "Dh", "Fastener head diameter", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Fastener Length", "L", "Fastener length", GH_ParamAccess.item, 0);
            pManager.AddNumberBoolean("Smooth Boolean", "Smooth", "True for smooth nails, false for other", GH_ParamAccess.item, 0); //nails
            pManager.AddNumberParameter("Fastener Fu", "Fu", "Fastener steel tensile ultimate strength", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Force(), "Force", "Force", "Nodal Force", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {


            string Ftype = "Ftype?";
            double D = 0;
            double Ds = 0;
            double Dh = 0;
            double L = 0;
            bool Smooth = true;
            double Fu = 0;

            DA.GetData(0, ref Ftype);
            DA.GetData(1, ref D);
            DA.GetData(2, ref Ds);
            DA.GetData(3, ref Dh);
            DA.GetData(4, ref L);
            DA.GetData(5, ref Smooth);
            DA.GetData(6, ref Fu);

            Fastener Fastener = new Fastener();
            if (Ftype = "dowel") {  Fastener=new Fastener(Ftype,D,L,Fu)}
            else if (Ftype = "bolt") { Fastener = new Fastener(Ftype, D, Dh, L, Fu)}
            else if (Ftype = "nail") { Fastener = new Fastener(Ftype, D, Dh, L, Smooth, Fu)}
            else if (Ftype = "screw") {  Fastener = new Fastener(Ftype, D, Ds, Dh, L, Fu)}




            double N = 0;
            double Vy = 0;
            double Vz = 0;
            double Mx = 0;
            double My = 0;
            double Mz = 0;
            string type = "";
            DA.GetData(0, ref N);
            DA.GetData(1, ref Vy);
            DA.GetData(2, ref Vz);
            DA.GetData(3, ref Mx);
            DA.GetData(4, ref My);
            DA.GetData(5, ref Mz);
            DA.GetData(6, ref type);
            Force force = new Force(N, Vy, Vz, Mx, My, Mz, type);

            DA.SetData(0, new GH_Force(force));
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
            get { return new Guid("ce157802-fa6c-44f1-846b-b4c6092649cf"); }
        }
    }
}