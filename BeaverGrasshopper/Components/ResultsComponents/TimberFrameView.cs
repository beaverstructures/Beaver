using BeaverCore.CrossSection;
using BeaverCore.Frame;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BeaverCore.Actions;
using BeaverCore.Misc;

namespace BeaverGrasshopper.Components.ResultsComponents
{
    public class TimberFrameView : GH_Component, IGH_VariableParameterComponent
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
        ULSCombinations uls_comb = new ULSCombinations();
        SLSCombinations sls_comb = new SLSCombinations();
        bool value_list_updated = false;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_TimberFrame(), "Timber Frames", "TF's", "Timber Frames to visualize", GH_ParamAccess.list);
            pManager.AddTextParameter("Utilization Type", "UType", "Utilization Type to Preview", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.list);
            pManager.AddTextParameter("Legend T", "T", "Legend T", GH_ParamAccess.list);
            pManager.AddColourParameter("Legend C", "C", "Legend C", GH_ParamAccess.list);
            pManager.AddNumberParameter("Maximum Utilization", "MaxUtil", "Maximum Utilization", GH_ParamAccess.item);
        }
        UtilizationType util_type = UtilizationType.All;
        SLSOptions sls_options = SLSOptions.Inst;
        ULSDirection uls_dir = ULSDirection.All;
        bool update_field_2 = false;
        bool update_field_3 = false;
        public override void AddedToDocument(GH_Document document)
        {
            if (Params.Input[1].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate utilization type list
                var vl = new GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "Type";
                //clear default contents
                vl.ListItems.Clear();
                foreach (UtilizationType util_type in Enum.GetValues(typeof(UtilizationType)))
                    vl.ListItems.Add(new GH_ValueListItem(util_type.ToString(), "\"" + util_type.ToString() + "\""));
                document.AddObject(vl, false);
                Params.Input[1].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[1].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 250, currPivot.Y - 11);
            }
            base.AddedToDocument(document);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            meshes.Clear();
            List<GH_TimberFrame> gh_timber_frames = new List<GH_TimberFrame>();
            string type = "";
            string sls_type_string = "";
            string uls_dir_string = "";
            string loadcase_string = "";
            int loadcase_index = -1;
            DA.GetDataList(0, gh_timber_frames);
            uls_comb = gh_timber_frames[0].Value.TimberPointsMap[0].ULSComb;
            sls_comb = gh_timber_frames[0].Value.TimberPointsMap[0].SLSComb;
            DA.GetData(1, ref type);
            UtilizationType new_util_type = (UtilizationType)Enum.Parse(typeof(UtilizationType), type, true);
            if (update_util(util_type,new_util_type))
            {
                update_field_2 = true;
                update_field_3 = true;
                util_type = new_util_type;
                return;
            }
            else
            {
                util_type = new_util_type;
                if (Params.Input.Count == 4)
                {
                    if (util_type == UtilizationType.SLS)
                    {
                        DA.GetData(2, ref sls_type_string);
                        SLSOptions new_sls_options = (SLSOptions)Enum.Parse(typeof(SLSOptions), sls_type_string, true);
                        if (new_sls_options != sls_options) { sls_options = new_sls_options; update_field_3 = true; return; }
                    }
                    else
                    {
                        DA.GetData(2, ref uls_dir_string);
                        ULSDirection new_uls_dir = (ULSDirection)Enum.Parse(typeof(ULSDirection), uls_dir_string, true);
                        uls_dir = new_uls_dir;
                    }
                    DA.GetData(3, ref loadcase_string);
                    loadcase_index = Int32.Parse(loadcase_string);

                }
            }
            double max_util = 0;
            foreach (GH_TimberFrame gh_timber_frame in gh_timber_frames)
            {
                TimberFrame timber_frame = gh_timber_frame.Value;
                Mesh mesh = MeshfromTimberFrame(timber_frame, util_type, uls_dir, sls_options, loadcase_index,ref max_util);
                meshes.Add(mesh);
            }
            DA.SetDataList(0, meshes);
            DA.SetData(3, max_util);

        }


        protected override void AfterSolveInstance()
        {
            if (value_list_updated)
            {
                ExpireSolution(true);
                value_list_updated = false;
            }
            VariableParameterMaintenance();
            if (value_list_updated)
            {
                ExpireSolution(true);
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
        #region COLORMESHING

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

        Color getColorfromValue(double minV, double maxV, double V, List<Color> colors)
        {
            double value = (V - minV) / (maxV - minV);
            return Utils.colorInterpolation(colors, value);
        }

        public Mesh MeshfromTimberFrame(TimberFrame timber_frame, UtilizationType util_type, ULSDirection dir, SLSOptions sls, int loadcase_index,ref double max_util)
        {
            List<Color> colors = new List<Color>() {
                Color.FromArgb(165, 0, 38), Color.FromArgb(215,48,39),
                Color.FromArgb(244,109,67), Color.FromArgb(253,174,97),
                Color.FromArgb(254,224,144), Color.FromArgb(255,255,191),
                Color.FromArgb(224,243,248), Color.FromArgb(171,217,233),
                Color.FromArgb(116,173,209), Color.FromArgb(69,117,180),
                Color.FromArgb(49,54,149) };
            colors.Reverse();
            double new_max_util = 0;
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
                    UtilizationResult util_result = RetrieveUtilization(frame_point, ref util_type, dir, sls, loadcase_index);
                    double util = util_result.util;
                    if (util > new_max_util) new_max_util = util;
                    Color color = getColorfromValue(0, 1, util,colors);
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
            if (max_util < new_max_util) max_util = new_max_util;
            return output;
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
        
        public bool update_util(UtilizationType util_type, UtilizationType new_util_type)
        {
            if (util_type == UtilizationType.All) { 
                if (new_util_type != UtilizationType.All) return true;

                else return false;

            }
            else if (util_type == UtilizationType.SLS)
            {
                if (new_util_type != UtilizationType.SLS) return true;

                else return false;

            }
            else
            {
                if (new_util_type != UtilizationType.SLS && new_util_type != UtilizationType.All) return false;
                else return true;
            }
        }
        
        #endregion

        #region GATHERUTILIZATION
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
                UtilizationResult max_result_y = maxULSUtilization(utilsUY, util_type, load_case_index);
                UtilizationResult max_result_z = maxULSUtilization(utilsUZ, util_type, load_case_index);
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
                UtilizationResult sls_result = maxSLSUtilizations(frame_point, sls, load_case_index);
                if (max_result.util < sls_result.util)
                {
                    max_result = sls_result;
                    util_type = UtilizationType.SLS;
                }
            }
            else if (util_type == UtilizationType.SLS)
            {
                max_result = maxSLSUtilizations(frame_point, sls, load_case_index);
            }
            else
            {
                if (dir == ULSDirection.All)
                {
                    List<double[]> utilsUY = RetrieveULSUtilization(frame_point, ULSDirection.Y);
                    List<double[]> utilsUZ = RetrieveULSUtilization(frame_point, ULSDirection.Z);
                    UtilizationResult max_result_y = maxULSUtilization(utilsUY, util_type, load_case_index);
                    UtilizationResult max_result_z = maxULSUtilization(utilsUZ, util_type, load_case_index);
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
                    max_result = maxULSUtilization(utilsU, util_type, load_case_index);
                }
            }
            return max_result;
        }

        UtilizationResult maxULSUtilization(List<double[]> utilsU, UtilizationType util_type, int load_case_index)
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
                case UtilizationType.AllULS:
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

                case UtilizationType.Tension:
                    loadcase_count = 0;
                    util_index = 0;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[0]) { max_util = utils[0]; load_index = loadcase_count; }
                        loadcase_count++;
                    }
                    break;

                case UtilizationType.Compression:
                    loadcase_count = 0;
                    util_index = 1;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[1]) { max_util = utils[1]; load_index = loadcase_count; }
                        loadcase_count++;
                    }
                    break;

                case UtilizationType.BiaxialBending:
                    loadcase_count = 0;
                    util_index = 2;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[2]) { max_util = utils[2]; load_index = loadcase_count; }
                        loadcase_count++;
                    }
                    break;

                case UtilizationType.Shear:
                    loadcase_count = 0;
                    util_index = 3;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[3]) { max_util = utils[3]; load_index = loadcase_count; }
                        loadcase_count++;
                    }
                    break;

                case UtilizationType.Torsion:
                    loadcase_count = 0;
                    util_index = 4;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[4]) { max_util = utils[4]; load_index = loadcase_count; }
                        loadcase_count++;
                    }
                    break;
                case UtilizationType.TensionAndBending:
                    loadcase_count = 0;
                    util_index = 5;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[5]) { max_util = utils[5]; load_index = loadcase_count; }
                        loadcase_count++;
                    }
                    break;
                case UtilizationType.CompressionAndBending:
                    loadcase_count = 0;
                    util_index = 6;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[6]) { max_util = utils[6]; load_index = loadcase_count; }
                        loadcase_count++;
                    }
                    break;
                case UtilizationType.CompressionAndBendingColumn:
                    loadcase_count = 0;
                    util_index = 7;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[7]) { max_util = utils[7]; load_index = loadcase_count; }
                        loadcase_count++;
                    }
                    break;
                case UtilizationType.CompressionAndBendingBeam:
                    loadcase_count = 0;
                    util_index = 8;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[8]) { max_util = utils[8]; load_index = loadcase_count; }
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

        UtilizationResult maxSLSUtilization(List<double> utilsS, int load_case_index)
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

        UtilizationResult maxSLSUtilizations(TimberFramePoint frame_point, SLSOptions sls, int load_case_index)
        {
            UtilizationResult result = new UtilizationResult();
            List<double> UtilS = new List<double>();
            switch (sls)
            {
                case SLSOptions.All:
                    UtilS = RetrieveSLSUtilization(frame_point, SLSOptions.Inst);
                    UtilizationResult result_inst = maxSLSUtilization(UtilS, load_case_index);
                    UtilS = RetrieveSLSUtilization(frame_point, SLSOptions.Fin);
                    UtilizationResult result_fin = maxSLSUtilization(UtilS, load_case_index);
                    result_fin.util_index = 1;
                    UtilS = RetrieveSLSUtilization(frame_point, SLSOptions.NetFin);
                    UtilizationResult result_netfin = maxSLSUtilization(UtilS, load_case_index);
                    result_netfin.util_index = 2;
                    List<UtilizationResult> results = new List<UtilizationResult>() { result_inst, result_fin, result_netfin };
                    result = results.OrderByDescending(item => item.util).FirstOrDefault();
                    break;

                case SLSOptions.Inst:
                    UtilS = RetrieveSLSUtilization(frame_point, sls);
                    result = maxSLSUtilization(UtilS, load_case_index);
                    break;
                case SLSOptions.Fin:
                    UtilS = RetrieveSLSUtilization(frame_point, sls);
                    result = maxSLSUtilization(UtilS, load_case_index);
                    result.util_index = 1;
                    break;
                case SLSOptions.NetFin:
                    UtilS = RetrieveSLSUtilization(frame_point, sls);
                    result = maxSLSUtilization(UtilS, load_case_index);
                    result.util_index = 2;
                    break;
            }

            return result;
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

        #region VARIABLE COMPONENT INTERFACE IMPLEMENTATION
        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {

            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {

            // Has to return a parameter object!
            Param_String param = new Param_String();
            string name = "";
            string nickname = "";
            string description = "";

            if (index == 2)
            {
                if (util_type == UtilizationType.SLS)
                {
                    name = "SLS Options";
                    nickname = "Options";
                    description = "SLS Results Options";
                }
                else
                {
                    name = "ULS Direction";
                    nickname = "Direction";
                    description = "ULS Directions Options";
                }
            }
            else if (index == 3)
            {
                name = "Load Case Index";
                nickname = "LCindex";
                description = "Load Case Index to Show";
            }

            param.Name = name;
            param.NickName = nickname;
            param.Description = description;
            param.Optional = true;
            return param;


        }


        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            //This function will be called when a parameter is about to be removed. 
            //You do not need to do anything, but this would be a good time to remove 
            //any event handlers that might be attached to the parameter in question.


            return true;
        }

        public void VariableParameterMaintenance()
        {
            //This method will be called when a closely related set of variable parameter operations completes. 
            //This would be a good time to ensure all Nicknames and parameter properties are correct. This method will also be 
            //called upon IO operations such as Open, Paste, Undo and Redo.


            //throw new NotImplementedException();
            
            if (Grasshopper.Instances.ActiveCanvas.Document != null)
            {
                GH_Document document = Grasshopper.Instances.ActiveCanvas.Document;
                if (util_type != UtilizationType.All)
                {
                    if (update_field_2)
                    {
                        if (Params.Input.Count > 2)
                        {
                            foreach (var source in Params.Input[3].Sources)
                            {
                                document.RemoveObject(source, true);
                            }
                            foreach (var source in Params.Input[2].Sources)
                            {
                                document.RemoveObject(source, true);
                            }
                            Params.Input[3].Sources.Clear();
                            Params.UnregisterInputParameter(Params.Input[3]);
                            Params.Input[2].Sources.Clear();
                            Params.UnregisterInputParameter(Params.Input[2]);
                        }
                        Params.OnParametersChanged();
                        if (Params.Input.Count == 2) Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 2));
                        if (util_type == UtilizationType.SLS) AddValueListforSLSOptions(this);
                        else AddValueListforULSDirection(this);
                        Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 3));
                        AddValueListforLoadCase(this);
                        Params.OnParametersChanged();
                        PointF currPivot = Params.Input[1].Attributes.Pivot;
                        //set the pivot of the new object
                        Params.Input[1].Sources[0].Attributes.Pivot = new PointF(currPivot.X - 250, currPivot.Y - 14);
                        update_field_2 = false;
                        update_field_3 = false;
                        Params.OnParametersChanged();
                        value_list_updated = true;
                    }
                    else if (update_field_3)
                    {
                        foreach (var source in Params.Input[3].Sources)
                        {
                            document.RemoveObject(source, true);
                        }
                        Params.Input[3].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[3]);
                        Params.OnParametersChanged();
                        Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 3));
                        Params.OnParametersChanged();
                        AddValueListforLoadCase(this);
                        update_field_3 = false;
                        PointF currPivot = Params.Input[1].Attributes.Pivot;
                        //set the pivot of the new object
                        Params.Input[1].Sources[0].Attributes.Pivot = new PointF(currPivot.X - 250, currPivot.Y - 14);
                        Params.OnParametersChanged();
                        value_list_updated = true;
                    }
                }
                else
                {
                    if (Params.Input.Count > 2)
                    {
                        foreach (var source in Params.Input[3].Sources)
                        {
                            document.RemoveObject(source, true);
                        }
                        foreach (var source in Params.Input[2].Sources)
                        {
                            document.RemoveObject(source, true);
                        }
                        Params.OnParametersChanged();
                        Params.Input[3].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[3]);


                        Params.Input[2].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[2]);
                        Params.OnParametersChanged();
                        PointF currPivot = Params.Input[1].Attributes.Pivot;
                        //set the pivot of the new object
                        Params.Input[1].Sources[0].Attributes.Pivot = new PointF(currPivot.X - 250, currPivot.Y + 14);
                        Params.OnParametersChanged();
                        value_list_updated = true;
                    }
                }
            }
            

        }

        void AddValueListforSLSOptions(GH_Component component)
        {
            GH_Document document = Grasshopper.Instances.ActiveCanvas.Document;
            if (Params.Input[2].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate utilization type list
                var vl = new GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "";
                //clear default contents
                vl.ListItems.Clear();
                foreach (SLSOptions sls in Enum.GetValues(typeof(SLSOptions)))
                    vl.ListItems.Add(new GH_ValueListItem(sls.ToString(), "\"" + sls.ToString() + "\""));
                document.AddObject(vl, false);
                Params.Input[2].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[2].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 180, currPivot.Y - 22);
            }
            base.AddedToDocument(document);
        }

        void AddValueListforULSDirection(GH_Component component)
        {
            GH_Document document = Grasshopper.Instances.ActiveCanvas.Document;
            if (Params.Input[2].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate utilization type list
                var vl = new GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "";
                //clear default contents
                vl.ListItems.Clear();
                foreach (ULSDirection dir in Enum.GetValues(typeof(ULSDirection)))
                    vl.ListItems.Add(new GH_ValueListItem(dir.ToString(), "\"" + dir.ToString() + "\""));
                document.AddObject(vl, false);
                Params.Input[2].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[2].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 180, currPivot.Y - 22);
            }
            base.AddedToDocument(document);
        }

        void AddValueListforLoadCase(GH_Component component)
        {
            GH_Document document = Grasshopper.Instances.ActiveCanvas.Document;
            if (Params.Input[3].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate utilization type list
                var vl = new GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "";
                //clear default contents
                vl.ListItems.Clear();
                int cont = -1;
                vl.ListItems.Add(new GH_ValueListItem("All", cont.ToString()));
                cont++;
                if (util_type == UtilizationType.SLS)
                {
                    List<Displacement> disps = new List<Displacement>();
                    if (sls_options == SLSOptions.Inst) disps = sls_comb.CharacteristicDisplacements;
                    else disps = sls_comb.CreepDisplacements;
                    foreach (Displacement combination in disps)
                    {
                        string comb_string = combination.combination;
                        vl.ListItems.Add(new GH_ValueListItem(comb_string, cont.ToString()));
                        cont++;
                    }
                }
                else
                {
                    List<Force> forces = new List<Force>(uls_comb.DesignForces);
                    foreach (Force combination in forces)
                    {
                        string comb_string = combination.combination;
                        vl.ListItems.Add(new GH_ValueListItem(comb_string, cont.ToString()));
                        cont++;
                    }
                }
                document.AddObject(vl, false);
                Params.Input[3].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[3].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 180, currPivot.Y - 8);
            }
            base.AddedToDocument(document);
        }

        #endregion



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