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
    public static class ExtendedMethods
    {
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
            CompressionAndBendingBeam,
            TorsionAndShear
        }

        public enum SLSOptions
        {
            All,
            Inst,
            Fin,
            NetFin
        }
        #region COLORMESHING

        public static Color GetColorfromValue(double minV, double maxV, double V)
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

        public static Color GetColorfromValue(double minV, double maxV, double V, List<Color> colors)
        {
            double value = (V - minV) / (maxV - minV);
            return Utils.colorInterpolation(colors, value);
        }

        public static Color GetColorfromInfo(int index, List<Color> colors)
        {
            return colors[index];
        }

        public static Mesh MeshfromTimberFrame(this TimberFrame timber_frame,
                                               UtilizationType util_type,
                                               ULSDirection dir,
                                               SLSOptions sls,
                                               int loadcase_index,
                                               ref double max_util,
                                               List<Color> colors,
                                               string plotType)
        {
            Mesh output = new Mesh();
            int info = 99;
            double new_max_util = 0;
            Point3d point_0 = new Point3d(timber_frame.FrameAxis.start.x,
                                          timber_frame.FrameAxis.start.y,
                                          timber_frame.FrameAxis.start.z);
            Point3d point_1 = new Point3d(timber_frame.FrameAxis.end.x,
                                          timber_frame.FrameAxis.end.y,
                                          timber_frame.FrameAxis.end.z);
            Line frame_line = new Line(point_0, point_1);
            Curve curv = frame_line.ToNurbsCurve();
            Point3d[] point_list = { new Point3d() };
            double[] parameters = curv.DivideByCount(timber_frame.TimberPointsMap.Count - 1, true, out point_list);
            Polyline polyline = new Polyline(point_list);
            Curve polyline_curve = polyline.ToPolylineCurve();
            
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
                    timber_frame.TimberPointsMap[rel_pos[i]].util = util;
                    timber_frame.TimberPointsMap[rel_pos[i]].util_index = util_result.util_index;
                    if (util > new_max_util)
                    {
                        new_max_util = util;
                        info = util_result.util_index;
                    }
                    Color color = (plotType == "Utilization") ? 
                        GetColorfromValue(0, 1, util, colors) : 
                        GetColorfromInfo(util_result.util_index, colors);
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
                            new MeshFace(4 * (i - 1)    , 4 * i    , 4 * i + 1, 4 * (i - 1) + 1),
                            new MeshFace(4 * (i - 1) + 1, 4 * i + 1, 4 * i + 2, 4 * (i - 1) + 2),
                            new MeshFace(4 * (i - 1) + 2, 4 * i + 2, 4 * i + 3, 4 * (i - 1) + 3),
                            new MeshFace(4 * (i - 1) + 3, 4 * i + 3, 4 * i    , 4 * (i - 1))
                        };
                        output.Faces.AddFaces(meshfaces);

                        if (i == rel_pos.Count-1)
                        {
                            MeshFace meshface = new MeshFace(4 * i, 4 * i + 1, 4 * i + 2, 4 * i + 3);
                            output.Faces.AddFace(meshface);
                        }

                    }
                    crosec_curve.Transform(step_move);


                }
            }
            output.Compact();
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
        #endregion

        #region GATHERUTILIZATION
        public static bool Update_util(UtilizationType util_type, UtilizationType new_util_type)
        {
            if (util_type == UtilizationType.All)
            {
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
        public static List<double> RetrieveSLSUtilization(TimberFramePoint frame_point, SLSOptions sls)
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

        public static List<double[]> RetrieveULSUtilization(TimberFramePoint frame_point, ULSDirection dir)
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

        public static UtilizationResult RetrieveUtilization(
            TimberFramePoint frame_point, 
            ref UtilizationType util_type,
            ULSDirection dir = ULSDirection.All, 
            SLSOptions sls = SLSOptions.All, 
            int load_case_index = -1)
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
                    // util_type = UtilizationType.AllULS;
                }
                else
                {
                    max_result = max_result_z;
                    dir = ULSDirection.Z;
                    // util_type = UtilizationType.AllULS;
                }
                UtilizationResult sls_result = MaxSLSUtilizations(frame_point, sls, load_case_index);
                if (max_result.util < sls_result.util)
                {
                    max_result = sls_result;
                    // util_type = UtilizationType.SLS;
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

        public static UtilizationResult MaxULSUtilization(List<double[]> utilsU, UtilizationType util_type, int load_case_index)
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
                case UtilizationType.TorsionAndShear:
                    loadcase_count = 0;
                    util_index = 9;
                    foreach (double[] utils in utilsU)
                    {
                        if (max_util < utils[9]) { max_util = utils[9]; load_index = loadcase_count; }
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

        public static UtilizationResult MaxSLSUtilization(List<double> utilsS, int load_case_index)
        {
            int loadcase_count = 0;
            int load_index = -1;
            double max_util = 0;
            if (load_case_index >= 0)
                return new UtilizationResult(utilsS[load_case_index], load_case_index, 0);
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

        public static UtilizationResult MaxSLSUtilizations(TimberFramePoint frame_point, SLSOptions sls, int load_case_index)
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
    }
}
