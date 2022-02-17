using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

using BeaverCore.Actions;
using BeaverCore.Misc;
using BeaverCore.CrossSection;
using BeaverCore.Frame;

using static BeaverGrasshopper.Components.ResultsComponents.ExtendedMethods;

namespace BeaverGrasshopper.Components.ResultsComponents
{
    public class VisualizeCriticalCheck : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Vi0sualizeCriticalCheck class.
        /// </summary>
        public VisualizeCriticalCheck()
          : base("VisualizeCriticalCheck", "CriticalCheck",
              "Displays the critical utilization of the timber frames based in colors",
              "Beaver", "4.Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_TimberFrame(), "Timber Frames", "TF's", "Timber Frames to visualize", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.list);
            pManager.AddTextParameter("Legend T", "T", "Legend T", GH_ParamAccess.list);
            pManager.AddColourParameter("Legend C", "C", "Legend C", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does 0the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_TimberFrame> gh_timber_frames = new List<GH_TimberFrame>();
            List<Mesh> meshes = new List<Mesh>();
            List<String> legend = new List<String>() {
                    "ULS Tension parallel to the grain             ", // 0
                    "ULS Compression parallel to the grain         ", // 1
                    "ULS Biaxial Bending                           ", // 2
                    "ULS Shear                                     ", // 3
                    "ULS Torsion                                   ", // 4
                    "ULS Combined Tension and Bending              ", // 5
                    "ULS Columns - Combined bending and compression", // 6
                    "ULS Beams - Combined bending and compression  ", // 7
                    "SLS Instantaneous deflection                  ", // 8
                    "SLS Net final deflection                      ", // 9
                    "SLS Final deflection                          "  // 10
            };
            List<Color> colors = new List<Color>() {
                Color.FromArgb(141,211,199), // 0
                Color.FromArgb(255,255,179), // 1
                Color.FromArgb(190,186,218), // 2
                Color.FromArgb(251,128,114), // 3
                Color.FromArgb(128,177,211), // 4
                Color.FromArgb(253,180,98),  // 5
                Color.FromArgb(179,222,105), // 6
                Color.FromArgb(252,205,229), // 7
                Color.FromArgb(217,217,217), // 8
                Color.FromArgb(188,128,189), // 9
                Color.FromArgb(204,235,197)  // 10 
            };

            DA.GetDataList(0, gh_timber_frames);

            int loadcase_index = -1;

            double max_util = 0;
            foreach (GH_TimberFrame gh_timber_frame in gh_timber_frames)
            {
                TimberFrame timber_frame = gh_timber_frame.Value;
                Mesh mesh = timber_frame.MeshfromTimberFrame(
                    UtilizationType.All, 
                    ULSDirection.All, 
                    SLSOptions.All, 
                    loadcase_index,
                    ref max_util,
                    colors,
                    "Critical Check");
                meshes.Add(mesh);
            }

            DA.SetDataList(0, meshes);
            DA.SetDataList(1, legend);
            DA.SetDataList(2, colors);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.CriticalChecks;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a5991d3a-02a1-46fb-915a-8482fa7498b7"); }
        }
    }
}