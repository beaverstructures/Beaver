using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BeaverGrasshopper.Components.ConnectionComponents
{
    public class Comp_ConnectionResults : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Comp_ConnectionResults class.
        /// </summary>
        public Comp_ConnectionResults()
          : base("Connection Results", "ConnectionRes",
              "Displays the calculated results for the Beaver connection",
              "Beaver", "2. Connection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
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
                return Properties.Resources.ConnectonResults;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e16a0300-fc7d-4a5a-8e29-c081e39b3445"); }
        }
    }
}