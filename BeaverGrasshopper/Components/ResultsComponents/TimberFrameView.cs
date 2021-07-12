using BeaverCore.CrossSection;
using BeaverCore.Frame;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BeaverGrasshopper.Components.ResultsComponents
{
    public class TimberFrameView : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TimberFrameView class.
        /// </summary>
        public TimberFrameView()
          : base("TimberFrameView", "Nickname",
              "Description",
              "Beaver", "4.Results")
        {
        }

        List<Mesh> meshes = new List<Mesh>();

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
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            meshes.Clear();
            List<GH_TimberFrame> gh_timber_frames = new List<GH_TimberFrame>();
            DA.GetDataList(0, gh_timber_frames);
            foreach (GH_TimberFrame gh_timber_frame in gh_timber_frames)
            {
                TimberFrame timber_frame = gh_timber_frame.Value;
                Mesh mesh = MeshfromTimberFrame(timber_frame);
                meshes.Add(mesh);
            }
            DA.SetDataList(0, meshes);

        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (meshes.Count == 0) { return; }
            foreach (Mesh mesh in meshes)
            {
                args.Display.DrawMeshFalseColors(mesh);
            }

        }

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

        Color getColorfromValue(double minV, double maxV, double V)
        {
            Color c = new Color();
            double crange = (maxV - minV) / 4;
            if (crange == 0) { crange = 1; }
            //Blue to Cyan
            double maxcyan = minV + crange;
            if (V >= minV && V < maxcyan)
            {
                int green = (int)(255 * (V - minV) / crange);
                c = Color.FromArgb(0, green, 255);
            }
            //Cyan to Green
            double maxgreen = minV + 2 * crange;
            if (V >= maxcyan && V < maxgreen)
            {
                int blue = (int)(((255 * (maxcyan - V)) / crange) + 255);
                c = Color.FromArgb(0, 255, blue);
            }
            //Green to Yellow
            double maxyellow = minV + 3 * crange;
            if (V >= maxgreen && V < maxyellow)
            {
                int red = (int)(255 * (V - maxgreen) / crange);
                c = Color.FromArgb(red, 255, 0);
            }
            //Yellow to Red
            double maxred = minV + 4 * crange;
            if (V >= maxyellow && V <= maxred)
            {
                int green = (int)(((255 * (maxyellow - V)) / crange) + 255);
                c = Color.FromArgb(255, green, 0);
            }
            if (V > maxred)
            {
                c = Color.Black;
            }
            return c;
        }

        public Mesh MeshfromTimberFrame(TimberFrame timber_frame, UtilizationType util_type, ULSDirection dir)
        {
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
                    double util = RetrieveUtilization(frame_point, util_type);
                    Color color = getColorfromValue(0, 1, util);
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
                        List<MeshFace> meshfaces = new List<MeshFace>();
                        meshfaces.Add(new MeshFace(4 * (i - 1), 4 * i, 4 * i + 1, 4 * (i - 1) + 1));
                        meshfaces.Add(new MeshFace(4 * (i - 1) + 1, 4 * i + 1, 4 * i + 2, 4 * (i - 1) + 2));
                        meshfaces.Add(new MeshFace(4 * (i - 1) + 2, 4 * i + 2, 4 * i + 3, 4 * (i - 1) + 3));
                        meshfaces.Add(new MeshFace(4 * (i - 1) + 3, 4 * i + 3, 4 * i, 4 * (i - 1)));
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

        public double RetrieveUtilization(TimberFramePoint frame_point, UtilizationType util_type,
            ULSDirection dir = ULSDirection.All, SLSOptions sls = SLSOptions.All, int load_case_index = -1)
        {
            double max_util = 0;
            if (util_type == UtilizationType.All)
            {
            }
            else if (util_type == UtilizationType.SLS)
            {
                List<double> utilsS = RetrieveSLSUtilization(frame_point, sls);
                if (load_case_index >= 0)
                {
                    utilsS = new List<double>() { utilsS[load_case_index] };
                }
            }
            else
            {
                List<double[]> utilsU = RetrieveULSUtilization(frame_point, dir);
                if (load_case_index >= 0)
                {
                    utilsU = new List<double[]>() { utilsU[load_case_index] };
                }
                switch (util_type)
                {
                    case UtilizationType.AllULS :
                        
                        foreach (double[] utils in utilsU)
                        {
                            foreach (double util in utils)
                            {
                                if (max_util < util) max_util = util;
                            }
                        }
                        break;

                    case UtilizationType.All:
                        foreach (double[] utils in utilsU)
                        {
                            foreach (double util in utils)
                            {
                                if (max_util < util) max_util = util;
                            }
                        }
                        break;

                    case UtilizationType.Tension:
                        foreach (double[] utils in utilsU)
                        {
                                if (max_util < utils[0]) max_util = utils[0];
                        }
                        break;

                    case UtilizationType.Compression:
                        foreach (double[] utils in utilsU)
                        {
                            if (max_util < utils[1]) max_util = utils[1];
                        }
                        break;

                    case UtilizationType.BiaxialBending:
                        foreach (double[] utils in utilsU)
                        {
                            if (max_util < utils[2]) max_util = utils[2];
                        }
                        break;

                    case UtilizationType.Shear:
                        foreach (double[] utils in utilsU)
                        {
                            if (max_util < utils[3]) max_util = utils[3];
                        }
                        break;

                    case UtilizationType.Torsion:
                        foreach (double[] utils in utilsU)
                        {
                            if (max_util < utils[4]) max_util = utils[4];
                        }
                        break;
                    case UtilizationType.TensionAndBending:
                        foreach (double[] utils in utilsU)
                        {
                            if (max_util < utils[5]) max_util = utils[5];
                        }
                        break;
                    case UtilizationType.CompressionAndBending:
                        foreach (double[] utils in utilsU)
                        {
                            if (max_util < utils[6]) max_util = utils[6];
                        }
                        break;
                    case UtilizationType.CompressionAndBendingColumn:
                        foreach (double[] utils in utilsU)
                        {
                            if (max_util < utils[7]) max_util = utils[7];
                        }
                        break;
                    case UtilizationType.CompressionAndBendingBeam:
                        foreach (double[] utils in utilsU)
                        {
                            if (max_util < utils[8]) max_util = utils[8];
                        }
                        break;
                }

            }
            return max_util;
        }

        public struct UtilizationResult
        {
            double util;
            int load_case_index;
            ULSDirection dir;
            SLSOptions sls;
            UtilizationType type;

            public UtilizationResult(double util, int point_index, int load_case_index, SLSOptions sls) : this()
            {
                this.util = util;
                this.load_case_index = load_case_index;
                this.sls = sls;
            }

            public UtilizationResult(double util, int point_index, int load_case_index, ULSDirection dir, UtilizationType type) : this()
            {
                this.util = util;
                this.load_case_index = load_case_index;
                this.dir = dir;
                this.type = type;
            }
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.TimberFrameResults;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1d376903-8946-41bc-83f2-0ce50fe2ff61"); }
        }
    }
}