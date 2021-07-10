using BeaverCore.Actions;
using BeaverCore.CrossSection;
using BeaverCore.Frame;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeaverGrasshopper
{
    public class TimberFramePointComp : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TimberFramePoint class.
        /// </summary>
        public TimberFramePointComp()
          : base("TimberFramePoint", "FramePoint",
              "Definition of a single timber frame point",
              "Beaver", "1.Frame")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_CroSec(), "Cross Section", "CroSec", "Frame Cross Section", GH_ParamAccess.item);
            pManager.AddNumberParameter("Normal Force", "N", "Normal force at point", GH_ParamAccess.list, new List<double>() { 0 });
            pManager.AddNumberParameter("Shear Force Y", "Vy", "Shear force at point in y direction", GH_ParamAccess.list, new List<double>() { 0 });
            pManager.AddNumberParameter("Shear Force Z", "Vz", "Shear force at point in z direction", GH_ParamAccess.list, new List<double>() { 0 });
            pManager.AddNumberParameter("Torsional Moment", "Mt", "Normal force at point", GH_ParamAccess.list, new List<double>() { 0 });
            pManager.AddNumberParameter("Bending Moment Y", "My", "Normal force at point", GH_ParamAccess.list, new List<double>() { 0 });
            pManager.AddNumberParameter("Bending Moment Z", "Mz", "Normal force at point", GH_ParamAccess.list, new List<double>() { 0 });
            pManager.AddVectorParameter("Displacement Vector", "Disp.", "Vector of point displcaement", GH_ParamAccess.list, Vector3d.Zero);
            pManager.AddTextParameter("Force Type", "Ftype", "Action type according to Eurocode 0 Annex A.1", GH_ParamAccess.list, "P");
            pManager.AddNumberParameter("Buckling Lenght Y", "bL_y", "Normal force at point", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("Buckling Lenght Z", "bL_s", "Normal force at point", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("Span lenght", "sL", "Normal force at point", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("Pre camber", "Pcamber", "Pre camber", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Span Type", "Stype", "Simple span or cantilever", GH_ParamAccess.item, "SimpleSpan");
            pManager.AddNumberParameter("Span Limit Range", "Srange",
                "Parameter in domain [0-1] to set limits between range defined in EC5 Table 7.2", GH_ParamAccess.item, 0.5);
            pManager.AddIntegerParameter("Service Class", "SC", "Service Class according to EC5", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_TFPoint(), "TimberFramePoint", "TFPoint", "Timber Frame Point", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_CroSec ghcrosec = new GH_CroSec();
            List<double> N = new List<double>();
            List<double> Vy = new List<double>();
            List<double> Vz = new List<double>();
            List<double> Mx = new List<double>();
            List<double> My = new List<double>();
            List<double> Mz = new List<double>();
            List<Vector3d> displacement = new List<Vector3d>();
            List<string> force_type = new List<string>();
            double bl_y = 0;
            double bl_z = 0;
            double sl = 0;
            double pre_camber = 0;
            string span_Type = "";
            double span_range = 0;
            int service_class = 0;
            DA.GetData(0, ref ghcrosec);
            DA.GetDataList(1, N);
            DA.GetDataList(2, Vy);
            DA.GetDataList(3, Vz);
            DA.GetDataList(4, Mx);
            DA.GetDataList(5, My);
            DA.GetDataList(6, Mz);
            DA.GetDataList(7, displacement);
            DA.GetDataList(8, force_type);
            DA.GetData(9, ref bl_y);
            DA.GetData(10, ref bl_z);
            DA.GetData(11, ref sl);
            DA.GetData(12, ref pre_camber);
            DA.GetData(13, ref span_Type);
            DA.GetData(14, ref span_range);
            DA.GetData(15, ref service_class);
            CroSec crosec = ghcrosec.Value;
            List<Force> forces = new List<Force>();
            List<Displacement> displacements = new List<Displacement>();
            if (!(new[] { N, Vy, Vz, Mx, My, Mz}).All(list => list.Count == N.Count) || N.Count!=displacement.Count || N.Count!=force_type.Count) {
                throw new ArgumentException("Input forces, displacements and type must be lists of equal lenght"); 
            }
            for (int i = 0; i < N.Count; i++)
            {
                Force force = new Force(N[i], Vy[i], Vz[i], Mx[i], My[i], Mz[i], force_type[i]);
                forces.Add(force);
                Displacement disp = new Displacement(displacement[i].X, displacement[i].Y, displacement[i].Z, force_type[i]);
                displacements.Add(disp);
            }
            //Think of ways of simplifying this input, it still needs span type and span limits
            TimberFramePoint timber_frame_point = new TimberFramePoint(forces, displacements, crosec, service_class, bl_y, bl_z, sl, 0.9);

            DA.SetData(0, new GH_TimberFramePoint(timber_frame_point));
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
            get { return new Guid("b6fb1405-42ff-4131-a1dd-b55b170cd60d"); }
        }
    }
}