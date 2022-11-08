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

using static BeaverGrasshopper.Components.Utilities.ResultUtilities;

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
              "Beaver", "4. Results")
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
            pManager.AddParameter(new Param_TimberFrame(), "Timber Frames", "TF's", "Modified timber frames", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.list);
            pManager.AddTextParameter("Legend T", "T", "Legend Text", GH_ParamAccess.list);
            pManager.AddColourParameter("Legend C", "C", "Legend Colors", GH_ParamAccess.list);
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
                    "0:  ULS Tension parallel to the grain             ", // 0
                    "1:  ULS Compression parallel to the grain         ", // 1
                    "2:  ULS Biaxial Bending                           ", // 2
                    "3:  ULS Shear                                     ", // 3
                    "4:  ULS Torsion                                   ", // 4
                    "5:  ULS Combined Tension and Bending              ", // 5
                    "6:  ULS Columns - Combined bending and compression", // 6
                    "7:  ULS Beams - Combined bending and compression  ", // 7
                    "8:  ULS Combined shear and torsion                ", // 8
                    "9:  SLS Instantaneous deflection                  ", // 9
                    "10: SLS Net final deflection                      ", // 10
                    "11: SLS Final deflection                          "  // 11
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
                Color.FromArgb(204,235,197), // 10 
                Color.FromArgb(255,237,111)  // 11
            };

            DA.GetDataList(0, gh_timber_frames);

            int loadcase_index = -1;

            double max_util = 0;
            List<GH_TimberFrame> out_beams = new List<GH_TimberFrame>();
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
                out_beams.Add(new GH_TimberFrame(timber_frame));
            }
            DA.SetDataList(0, gh_timber_frames);
            DA.SetDataList(1, meshes);
            DA.SetDataList(2, legend);
            DA.SetDataList(3, colors);

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