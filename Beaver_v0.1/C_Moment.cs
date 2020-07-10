using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Beaver_v0._1
{
    public class C_Moment : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public C_Moment()
          : base("Moment Resultant Force", "Moment",
              "Evaluates forces acting on each dowel on moment-resistant connections",
              "Beaver", "Connections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Fasteners", "Fasteners", "Coordinate [mm] of Fasteners considering (0,0,0) as moment origin (only X and Y coordinates will be considered)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Diameter", "d", "Dowel diameter [mm]", GH_ParamAccess.item,8);
            pManager.AddNumberParameter("Mean Timber Density", "ρm", "Mean Timber Density [kg/m³] (influences on estimated Kser)", GH_ParamAccess.item, 400);
            pManager.AddNumberParameter("Design Moment", "Md", "Design Moment (N.mm)", GH_ParamAccess.item,0);
            pManager.AddNumberParameter("Design Y Load", "Fyd", "Design Load in Y direction [N]", GH_ParamAccess.item,0);
            pManager.AddNumberParameter("Design X Load", "Fxd", "Deisgn Load in X direction [N]", GH_ParamAccess.item,0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Design Load", "Fved", "Design Load of each dowel [N]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Alpha", "α", "Angle of Load parallel to the fiber [rad]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Direction", "d", "Direction of the load on XY Plane", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rotational Stiffness", "Kφ", "Rotational Stiffness of the whole connection (per shear plane) [Nmm/rad]", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> dowels = new List<Point3d>();
            double d = 0;
            double pm = 0;
            double Md = 0;
            double Vd = 0;
            double Hd = 0;
            DA.GetDataList(0, dowels);
            DA.GetData(1, ref d);
            DA.GetData(2, ref pm);
            DA.GetData(3, ref Md);
            DA.GetData(4, ref Vd);
            DA.GetData(5, ref Hd);
            List<double> r = new List<double>();
            List<double> Fd = new List<double>();
            List<double> alpha = new List<double>();
            List<double> beta = new List<double>();
            List<Vector3d> dir = new List<Vector3d>();
            double somar2 = 0;
            double n = dowels.Count;
            double fhd = Hd / n;
            double fvd = Vd / n;
            Point3d o = new Point3d(0, 0, 0);
            foreach (Point3d pt in dowels)
            {
                double r2 = Math.Pow(pt.X, 2) + Math.Pow(pt.Y, 2);
                r.Add(Math.Sqrt(r2));
                beta.Add(Math.Atan(pt.Y / pt.X));
                somar2 += r2;
            }
            for (int i = 0; i < dowels.Count; i++)
            {
                Vector3d di = dowels[i] - o;
                Vector3d rvec = Vector3d.CrossProduct(-Vector3d.ZAxis, di);
                rvec.Unitize();
                dir.Add(rvec);
                double fm = Md * r[i] / somar2;
                double ft = Math.Sqrt(Math.Pow(fvd + fm * Math.Cos(beta[i]), 2) + Math.Pow(fhd + fm * Math.Sin(beta[i]), 2));
                Fd.Add(ft);
                alpha.Add(Math.Acos((fhd + fm * Math.Sin(beta[i])) / ft));
            }
            DA.SetDataList(0, Fd);
            DA.SetDataList(1, alpha);
            double Kser = Math.Pow(pm, 1.5) * d / 23;
            double kser = Kser * somar2;
            DA.SetData(3, kser);
            DA.SetDataList(2, dir);


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
                return Properties.Resources._3_7;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c28f8edf-f3ef-4e45-b710-5929da5958b4"); }
        }
    }
}