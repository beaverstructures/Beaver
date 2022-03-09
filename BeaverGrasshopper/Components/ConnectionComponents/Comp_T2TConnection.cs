using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BeaverGrasshopper.Components.ConnectionComponents
{
    public class Comp_T2TConnection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Comp_T2TConnection()
          : base("Timber to Timber Connection", "T2T",
              "Assembles a timber to timber connection",
              "Beaver", "2. Connection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Fastener(), "Fastener", "Fast", "Beaver fastener element", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Spacing(), "Spacing", "Spacing", "Beaver spacing element", GH_ParamAccess.item);
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
                return Properties.Resources.TimberToTimberConnection;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2fc82eb2-7248-444f-98a2-51f77eef0df0"); }
        }
    }
}