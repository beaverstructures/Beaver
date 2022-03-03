using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BeaverGrasshopper.Components.ConnectionComponents
{
    public class Comp_T2SConnection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Comp_T2SConnection class.
        /// </summary>
        public Comp_T2SConnection()
          : base("Timber to Steel Connection", "T2S",
              "Assembles a timber to steel Beaver connection",
              "Beaver", "2. Connection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Fastener(), "Fastener", "Fast", "Beaver fastener element", GH_ParamAccess.item);
            pManager.AddNumberParameter("Npar", "Npar", "Number of perpendicular fasteners", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Nperp", "Nperp", "Number of perpendicular fasteners", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("horizontal rotation", "XYrot", " rotation position of the connection", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Vertical rotation", "XZrot", "position of the connection", GH_ParamAccess.item, 0);
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
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.SteelToTimberConnection;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a8637843-b587-4653-9470-7ce1151d2391"); }
        }
    }
}