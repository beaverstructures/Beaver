using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Karamba.Models;
using Karamba.GHopper.Models;

using BeaverCore.Frame;
using Karamba.Elements;

namespace BeaverGrasshopper
{
    public class KarambatoBeaver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the KarambatoBeaver class.
        /// </summary>
        public KarambatoBeaver()
          : base("KarambatoBeaver", "Karamba",
              "Retrieves TimberFrames from Karamba beams",
              "Beaver", "External")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Model_in", "Model_in",
                        "Karamba model", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_TimberFrame(), "TimberFrames", "TF's", "Timber Frames from Karamba beams", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Model gh_model = new GH_Model();
            DA.GetData(0, ref gh_model);
            Model model = gh_model.Value;
            List<ModelElement> elements = model.elems;
            List<ModelBeam> beams = (List<ModelBeam>)elements.Where(element => element is ModelBeam);
            int i = 0;
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
            get { return new Guid("4d798265-19cd-4524-8b9a-2f36f6994d81"); }
        }
    }
}