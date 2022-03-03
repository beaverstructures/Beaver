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
          : base("Fastener", "Fast",
              "Creates Beaver fastener element for assembly into a Beaver connection element",
              "Beaver", "2. Connection")
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
            pManager.AddBooleanParameter("Smooth Boolean", "Smooth", "True for smooth nails, false for other", GH_ParamAccess.item, false); //nails
            pManager.AddNumberParameter("Fastener Fu", "Fu", "Fastener steel tensile ultimate strength", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Fastener(), "Fastener", "Fast", "Beaver fastener element", GH_ParamAccess.item);
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

            Fastener fastener = new Fastener(Ftype,D,Ds,Dh,L,Fu,Smooth);
            DA.SetData(0, new GH_Fastener(fastener));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Bolt;
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