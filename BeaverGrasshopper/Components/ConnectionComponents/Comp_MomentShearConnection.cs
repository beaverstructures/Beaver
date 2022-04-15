using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using BeaverCore.Connections;
using BvGeom = BeaverCore.Geometry;
using BeaverCore.Frame;
using BeaverGrasshopper.CoreWrappers;
using System.Drawing;
using static BeaverGrasshopper.Components.Utilities.RhinoUtilities;
using static BeaverGrasshopper.Components.Utilities.ResultUtilities;

namespace BeaverGrasshopper.Components.ConnectionComponents
{
    public class Comp_MomentShearConnection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Comp_MomentShearConnection class.
        /// </summary>
        public Comp_MomentShearConnection()
          : base("SemiRigidConnection", "SemiRigidConnection",
              "Assembles the connection spacings to be used in the connection analysis. Hypothesos of a SemiRigidConnection using a FREE CENTER OF ROTATION",
              "Beaver", "2. Connection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("plane", "plane", "plane", GH_ParamAccess.item, Plane.WorldZX);
            pManager.AddPointParameter("points", "points", "points with units in meters", GH_ParamAccess.list);
            pManager.AddParameter(new Param_Fastener(), "Fastener", "Fast", "Beaver fastener with calculated capacity from T2T or T2S components", GH_ParamAccess.item);
            pManager.AddParameter(new Param_ShearSpacing(), "Spacing", "Spacing", "Beaver Spacing element", GH_ParamAccess.item);
            pManager.AddParameter(new Param_TFPoint(), "TimberFramePoint", "TF", "Beaver TimberFramePoint element", GH_ParamAccess.item);
            pManager.AddTextParameter("ForceType", "FType", "ForceType", GH_ParamAccess.item);
            pManager.AddTextParameter("AnalysisType", "AType", "UtilizationType", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("points", "points", "points", GH_ParamAccess.list);
            pManager.AddVectorParameter("vector", "vector", "vector", GH_ParamAccess.list);
            pManager.AddNumberParameter("value", "value", "value", GH_ParamAccess.list);
            pManager.AddNumberParameter("utilization", "utilization", "utilization", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "Mesh", "Colored mesh with fastener utilizations", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane plane = new Plane();
            List<Point3d> points = new List<Point3d>();
            GH_Fastener gh_fastener = null;
            GH_ShearSpacing gh_spacing = null;
            GH_TimberFramePoint gh_tfPoint = null;
            double t1 = 0;
            double t2 = 0;
            string Ftype = "";
            string Atype = "";

            DA.GetData(0, ref plane);
            DA.GetDataList(1, points);
            DA.GetData(2, ref gh_fastener);
            DA.GetData(3, ref gh_spacing);
            DA.GetData(4, ref gh_tfPoint);
            DA.GetData(5, ref Ftype);
            DA.GetData(6, ref Atype);

            t1 = t1 / 1000;
            t2 = t2 / 1000;

            List<BvGeom.Point2D> bvPoints = new List<BvGeom.Point2D>();
            List<Vector3d> out_vectors = new List<Vector3d>();
            List<BvGeom.Vector3D> bvVectors = new List<BvGeom.Vector3D>();
            List<double> out_values = new List<double>();
            List<double> out_utilizations = new List<double>();
            List<Mesh> out_meshes = new List<Mesh>();
            List<Color> colors = new List<Color>() {
                Color.Black,
                Color.FromArgb(165, 0, 38),
                Color.FromArgb(215,48,39),
                Color.FromArgb(244,109,67),
                Color.FromArgb(253,174,97),
                Color.FromArgb(254,224,144),
                Color.FromArgb(255,255,191),
                Color.FromArgb(224,243,248),
                Color.FromArgb(171,217,233),
                Color.FromArgb(116,173,209),
                Color.FromArgb(69,117,180),
                Color.FromArgb(49,54,149) };
            colors.Reverse();
            List<String> legend = new List<String>() {
                "0%","10%","20%","30%","40%","50%","60%","70%","80%","90%","100%",">100%"
            };

            Transform localtransform = Transform.ChangeBasis(Plane.WorldXY, plane);
            Transform globaltransform = Transform.ChangeBasis(plane, Plane.WorldXY);
            // assembles connection
            foreach (Point3d point in points)
            {
                point.Transform(localtransform);
                bvPoints.Add(new BvGeom.Point2D(point.X, point.Y));
            }
            ConnectionMoment connectionMoment = new ConnectionMoment(
                gh_fastener.Value, 
                gh_tfPoint.Value.Forces[0],
                plane.RhinoPlane2Beaver(),
                bvPoints,
                gh_spacing.Value,
                gh_tfPoint.Value.sc);

            connectionMoment.Initialize();

            // returns force vectors and utilizations into Rhino format
            for (int i = 0; i < connectionMoment.FastenerList.Count; i++)
            {
                bvVectors.Add(connectionMoment.FastenerList[i].forces[Ftype]);
                Vector3d vector = new Vector3d(bvVectors[i].x, bvVectors[i].y, bvVectors[i].z);
                vector.Transform(globaltransform);
                out_vectors.Add(vector);
                out_utilizations.Add(connectionMoment.FastenerList[i].utilization[Atype]);

                // Create colored pipe mesh
                Line fastenerline = new Line(points[i], plane.ZAxis, gh_fastener.Value.l / 1000);
                fastenerline.Transform(Transform.Translation(new Vector3d(plane.ZAxis * -1 * (gh_fastener.Value.l / 2000))));
                out_meshes.Add(Mesh.CreateFromCurvePipe(new LineCurve(fastenerline), gh_fastener.Value.d / 1000, 8, 1, MeshPipeCapStyle.Flat, false, null));
                Color color = GetColorfromValue(0, 1, out_utilizations[i], colors);
                for (int j = 0; j < out_meshes[i].Vertices.Count; j++)
                {
                    out_meshes[i].VertexColors.Add(color);
                }
            }


            DA.SetDataList(0, points);
            DA.SetDataList(1, out_vectors);
            DA.SetDataList(2, out_values);
            DA.SetDataList(3, out_utilizations);
            DA.SetDataList(4, out_meshes);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.FastenerForces;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b5d9e2b4-97b6-4848-a5df-c1b3ae957aa6"); }
        }
    }
}