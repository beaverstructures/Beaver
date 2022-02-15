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

using BeaverGrasshopper.Components.ResultsComponents;

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

            DA.GetDataList(0, gh_timber_frames);

            int loadcase_index = -1;

            double max_util = 0;
            foreach (GH_TimberFrame gh_timber_frame in gh_timber_frames)
            {
                TimberFrame timber_frame = gh_timber_frame.Value;
                Mesh mesh = MeshfromTimberFrame(
                    timber_frame, 
                    UtilizationType.All, 
                    ULSDirection.All, 
                    SLSOptions.All, 
                    loadcase_index);
                meshes.Add(mesh);
            }

            DA.SetDataList(0, meshes);
            List<String> legend = new List<String>() {
                    "ULS Tension parallel to the grain             ", // 0
                    "ULS Compression parallel to the grain         ", // 1
                    "ULS Biaxial Bending                           ", // 2
                    "ULS Shear                                     ", // 3
                    "ULS Torsion                                   ", // 4
                    "ULS Combined Tension and Bending              ", // 5
                    "ULS Columns - Combined compression and bending", // 6
                    "ULS Beams - Combined bending and compression  ", // 7
                    "SLS Instantaneous deflection                  ", // 8
                    "SLS Net final deflection                      ", // 9
                    "SLS Final deflection                          "  // 10
            };
            DA.SetDataList(1, legend);

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

        

        public Mesh MeshfromTimberFrame(TimberFrame timber_frame, UtilizationType util_type, ULSDirection dir,
            SLSOptions sls, int loadcase_index)
        {
            List<Color> colors = new List<Color>() {
                Color.FromArgb(141,211,199), //0
                Color.FromArgb(255,255,179), //1
                Color.FromArgb(190,186,218), //2
                Color.FromArgb(251,128,114), //3
                Color.FromArgb(128,177,211), //4
                Color.FromArgb(253,180,98),  //5
                Color.FromArgb(179,222,105), //6
                Color.FromArgb(252,205,229), //7
                Color.FromArgb(217,217,217), //8
                Color.FromArgb(188,128,189), //9
                Color.FromArgb(204,235,197), //10
            };
            double new_max_util = 0;
            int info = 99;
            Point3d point_0 = new Point3d(timber_frame.FrameAxis.start.x, timber_frame.FrameAxis.start.y, timber_frame.FrameAxis.start.z);
            Point3d point_1 = new Point3d(timber_frame.FrameAxis.end.x, timber_frame.FrameAxis.end.y, timber_frame.FrameAxis.end.z);
            Line frame_line = new Line(point_0, point_1);
            Curve curv = frame_line.ToNurbsCurve();
            Point3d[] point_list = { new Point3d() };
            double[] parameters = curv.DivideByCount(timber_frame.TimberPointsMap.Count - 1, true, out point_list);
            Polyline polyline = new Polyline(point_list);
            Curve polyline_curve = polyline.ToPolylineCurve();
            Mesh output = new Mesh();
            double increment = curv.GetLength() / (point_list.Length - 1);
            if (timber_frame.TimberPointsMap[0].CS is CroSec_Circ)
            {
                CroSec_Circ circ_crosec = (CroSec_Circ)timber_frame.TimberPointsMap[0].CS;
                output = Mesh.CreateFromCurvePipe(polyline_curve, circ_crosec.d, 8, 1, MeshPipeCapStyle.Flat, false, null);
            }
            else if (timber_frame.TimberPointsMap[0].CS is CroSec_Rect)
            {
                CroSec_Rect rect_crosec = (CroSec_Rect)timber_frame.TimberPointsMap[0].CS;
                Polyline crosec_curve = new Rectangle3d(Plane.WorldXY, rect_crosec.b, rect_crosec.h).ToPolyline();
                Transform adjust_rect = Transform.Translation(new Vector3d(-0.5 * rect_crosec.b, -0.5 * rect_crosec.h, 0));
                crosec_curve.Transform(adjust_rect);
                Vector3d axis_vector = new Vector3d(point_1 - point_0);
                axis_vector.Unitize();
                Vector3d width_axis = Vector3d.CrossProduct(axis_vector, Vector3d.ZAxis);
                Vector3d height_axis = Vector3d.CrossProduct(width_axis, axis_vector);
                Plane crosec_plane = new Plane(point_0, width_axis, height_axis);
                Transform plane_to_plane = Transform.PlaneToPlane(Plane.WorldXY, crosec_plane);
                Transform step_move = Transform.Translation(increment * axis_vector);
                crosec_curve.Transform(plane_to_plane);
                List<CroSecPoints> mesh_nodes = new List<CroSecPoints>();
                List<double> rel_pos = new List<double>(timber_frame.TimberPointsMap.Keys);
                List<TimberFramePoint> frame_points = new List<TimberFramePoint>(timber_frame.TimberPointsMap.Values);
                crosec_curve.Remove(crosec_curve[4]);
                for (int i = 0; i < rel_pos.Count; i++)
                {
                    TimberFramePoint frame_point = frame_points[i];
                    Point3d[] crosec_points = crosec_curve.ToArray();
                    UtilizationResult util_result = RetrieveUtilization(frame_point,
                                                                        ref util_type,
                                                                        dir,
                                                                        sls,
                                                                        loadcase_index);
                    double util = util_result.util;
                    if (util > new_max_util)
                    {
                        new_max_util = util;
                        info = util_result.util_index;
                    }
                    Color color = GetColorfromInfo(info, colors);
                    CroSecPoints cs_points = new CroSecPoints(crosec_points, color);

                    foreach (Point3d point in crosec_points)
                    {
                        output.Vertices.Add(point);
                        output.VertexColors.Add(color);
                    }
                    if (i == 0)
                    {
                        MeshFace meshface = new MeshFace(0, 1, 2, 3);
                        output.Faces.AddFace(meshface);
                    }
                    else
                    {
                        List<MeshFace> meshfaces = new List<MeshFace>
                        {
                            new MeshFace(4 * (i - 1), 4 * i, 4 * i + 1, 4 * (i - 1) + 1),
                            new MeshFace(4 * (i - 1) + 1, 4 * i + 1, 4 * i + 2, 4 * (i - 1) + 2),
                            new MeshFace(4 * (i - 1) + 2, 4 * i + 2, 4 * i + 3, 4 * (i - 1) + 3),
                            new MeshFace(4 * (i - 1) + 3, 4 * i + 3, 4 * i, 4 * (i - 1))
                        };
                        output.Faces.AddFaces(meshfaces);

                        if (i == rel_pos.Count)
                        {
                            MeshFace meshface = new MeshFace(4 * i, 4 * i + 1, 4 * i + 2, 4 * i + 3);
                            output.Faces.AddFace(meshface);
                        }

                    }
                    crosec_curve.Transform(step_move);


                }
            }
            return output;
        }

        #region GATHER UTILIZATION

        public enum ULSDirection
        {
            All,
            Y,
            Z
        }

        public enum UtilizationType
        {
            All,
            SLS,
            AllULS,
            Tension,
            Compression,
            BiaxialBending,
            Shear,
            Torsion,
            TensionAndBending,
            CompressionAndBending,
            CompressionAndBendingColumn,
            CompressionAndBendingBeam
        }

        public enum SLSOptions
        {
            All,
            Inst,
            Fin,
            NetFin
        }

        public List<double> RetrieveSLSUtilization(TimberFramePoint frame_point, SLSOptions sls)
        {
            TimberFrameSLSResult utilsSLS = frame_point.SLSUtilization();
            List<double> utilsS = new List<double>();
            switch (sls)
            {
                case SLSOptions.Inst:
                    utilsS = utilsSLS.InstUtils;
                    break;

                case SLSOptions.Fin:
                    utilsS = utilsSLS.FinUtils;
                    break;

                case SLSOptions.NetFin:
                    utilsS = utilsSLS.NetFinUtils;
                    break;
            }
            return utilsS;
        }

        public List<double[]> RetrieveULSUtilization(TimberFramePoint frame_point, ULSDirection dir)
        {

            List<double[]> utilsU = new List<double[]>();
            TimberFrameULSResult utilsULS = frame_point.ULSUtilization();
            switch (dir)
            {
                case ULSDirection.Y:
                    utilsU = utilsULS.UtilsY;
                    break;

                case ULSDirection.Z:
                    utilsU = utilsULS.UtilsZ;
                    break;

            }

            return utilsU;
        }

        public UtilizationResult RetrieveUtilization(TimberFramePoint frame_point, ref UtilizationType util_type,
            ULSDirection dir = ULSDirection.All, SLSOptions sls = SLSOptions.All, int load_case_index = -1)
        {
            UtilizationResult max_result = new UtilizationResult();
            if (util_type == UtilizationType.All)
            {
                List<double[]> utilsUY = RetrieveULSUtilization(frame_point, ULSDirection.Y);
                List<double[]> utilsUZ = RetrieveULSUtilization(frame_point, ULSDirection.Z);
                UtilizationResult max_result_y = MaxULSUtilization(utilsUY, util_type, load_case_index);
                UtilizationResult max_result_z = MaxULSUtilization(utilsUZ, util_type, load_case_index);
                if (max_result_y.util > max_result_z.util)
                {
                    max_result = max_result_y;
                    dir = ULSDirection.Y;
                    util_type = UtilizationType.AllULS;
                }
                else
                {
                    max_result = max_result_z;
                    dir = ULSDirection.Z;
                    util_type = UtilizationType.AllULS;
                }
                UtilizationResult sls_result = MaxSLSUtilizations(frame_point, sls, load_case_index);
                if (max_result.util < sls_result.util)
                {
                    max_result = sls_result;
                    util_type = UtilizationType.SLS;
                }
            }
            else if (util_type == UtilizationType.SLS)
            {
                max_result = MaxSLSUtilizations(frame_point, sls, load_case_index);
            }
            else
            {
                if (dir == ULSDirection.All)
                {
                    List<double[]> utilsUY = RetrieveULSUtilization(frame_point, ULSDirection.Y);
                    List<double[]> utilsUZ = RetrieveULSUtilization(frame_point, ULSDirection.Z);
                    UtilizationResult max_result_y = MaxULSUtilization(utilsUY, util_type, load_case_index);
                    UtilizationResult max_result_z = MaxULSUtilization(utilsUZ, util_type, load_case_index);
                    if (max_result_y.util > max_result_z.util)
                    {
                        max_result = max_result_y;
                        dir = ULSDirection.Y;
                    }
                    else
                    {
                        max_result = max_result_z;
                        dir = ULSDirection.Z;
                    }
                }
                else
                {
                    List<double[]> utilsU = RetrieveULSUtilization(frame_point, dir);
                    max_result = MaxULSUtilization(utilsU, util_type, load_case_index);
                }
            }
            return max_result;
        }

        UtilizationResult MaxULSUtilization(List<double[]> utilsU, UtilizationType util_type, int load_case_index)
        {
            double max_util = 0;
            int load_index = -1;
            int util_index = 0;
            if (load_case_index >= 0)
            {
                utilsU = new List<double[]>() { utilsU[load_case_index] };
            }
            int util_cont = 0;
            int loadcase_count = 0;
            switch (util_type)
            {
                case UtilizationType.All:
                    loadcase_count = 0;
                    foreach (double[] utils in utilsU)
                    {
                        util_cont = 0;
                        foreach (double util in utils)
                        {
                            if (max_util < util)
                            {
                                max_util = util;
                                util_index = util_cont;
                                load_index = loadcase_count;
                            }
                            util_cont++;
                        }
                        loadcase_count++;
                    }
                    break;
            }
            if (load_case_index >= 0)
            {
                load_index = load_case_index;
            }
            return new UtilizationResult(max_util, load_index, util_index);
        }

        

        UtilizationResult MaxSLSUtilizations(TimberFramePoint frame_point, SLSOptions sls, int load_case_index)
        {
            UtilizationResult result = new UtilizationResult();
            List<double> UtilS = new List<double>();
            switch (sls)
            {
                case SLSOptions.All:
                    UtilS = RetrieveSLSUtilization(frame_point, SLSOptions.Inst);
                    UtilizationResult result_inst = MaxSLSUtilization(UtilS, load_case_index);
                    result_inst.util_index = 8;
                    UtilS = RetrieveSLSUtilization(frame_point, SLSOptions.Fin);
                    UtilizationResult result_fin = MaxSLSUtilization(UtilS, load_case_index);
                    result_fin.util_index = 10;
                    UtilS = RetrieveSLSUtilization(frame_point, SLSOptions.NetFin);
                    UtilizationResult result_netfin = MaxSLSUtilization(UtilS, load_case_index);
                    result_netfin.util_index = 9;
                    List<UtilizationResult> results = new List<UtilizationResult>() { result_inst, result_fin, result_netfin };
                    result = results.OrderByDescending(item => item.util).FirstOrDefault();
                    break;

                case SLSOptions.Inst:
                    UtilS = RetrieveSLSUtilization(frame_point, sls);
                    result = MaxSLSUtilization(UtilS, load_case_index);
                    result.util_index = 8;
                    break;
                case SLSOptions.Fin:
                    UtilS = RetrieveSLSUtilization(frame_point, sls);
                    result = MaxSLSUtilization(UtilS, load_case_index);
                    result.util_index = 10;
                    break;
                case SLSOptions.NetFin:
                    UtilS = RetrieveSLSUtilization(frame_point, sls);
                    result = MaxSLSUtilization(UtilS, load_case_index);
                    result.util_index = 9;
                    break;
            }

            return result;
        }

        UtilizationResult MaxSLSUtilization(List<double> utilsS, int load_case_index)
        {
            int loadcase_count = 0;
            int load_index = -1;
            double max_util = 0;
            if (load_case_index >= 0) return new UtilizationResult(utilsS[load_case_index], load_case_index, 0);
            foreach (double util in utilsS)
            {
                if (max_util < util)
                {
                    max_util = util;
                    load_index = loadcase_count;
                }
                loadcase_count++;
            }
            return new UtilizationResult(max_util, load_case_index, 0);
        }

        public struct UtilizationResult
        {
            public double util;
            public int load_case_index;
            public int util_index;

            public UtilizationResult(double util, int load_case_index, int util_index)
            {
                this.util = util;
                this.load_case_index = load_case_index;
                this.util_index = util_index;
            }
        }
        #endregion


        Color GetColorfromInfo(int index, List<Color> colors)
        {
            return colors[index];
        }

        public class CroSecPoints
        {
            Point3d[] pos;
            Color color;

            public CroSecPoints() { }

            public CroSecPoints(Point3d[] points, Color color)
            {
                pos = points;
                this.color = color;
            }
        }

    }
}