using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Beaver_v0._1
{
    public class C_Multiple : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public C_Multiple()
          : base("Multiple Fastener Resistance", "Multiple Fastener",
              "Calculates the overall resistance of multiple fastener connections considering the effective number of fasteners (nef)",
              "Beaver", "Connections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Fvsd", "Fvsd", "Shear Force on Connection or on Fastener [N] (define option in Method)", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Fvrd", "Fvrd", "Fastener Load Carrying Capacity [N] (considering all acting shear planes)", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Diameter", "d", "Force Angle related to the grain of t [rad]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Alpha", "α", "Force Angle related to the grain [rad]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Fastener Parallel", "npar", "Parallel number of Screws", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Fastener Perpendicullar", "npep", "Perpendicular number of Screws", GH_ParamAccess.item, 1);
            pManager.AddTextParameter("Fastener Type", "F.Type", "Fastener Type (bolt, dowel, screw or nail)", GH_ParamAccess.item, "bolt");
            pManager.AddBooleanParameter("InUtilidual or Total Action", "Method", "if true the value of Fvsd is considered as" +
                " the shear force acting on a specific fastener (i.e. the maximum force), if false the value is considered as " +
                "the shear force on the overall connection",GH_ParamAccess.item,false);
            pManager.AddNumberParameter("Parallel Spacing", "a1", " Fastener spacing parallel to the grain [mm]", GH_ParamAccess.item, 100);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Shear Strenght", "Rcvd", "Connection Design Load Carrying Capacity [N]");
            pManager.Register_DoubleParam("Util", "Util", "Ratio between Stress and Strength");
            pManager.Register_DoubleParam("Effective Number of Fasteners", "Nef", "Effective Number of Fasteners");
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double vsd = 0;
            double fvrd = 0;
            double alpha = 0;
            double npar = 0;
            double npep = 0;
            double d = 0;
            string type = "";
            bool method = false;
            double a1 = 0;
            if (!DA.GetData<double>(0, ref vsd)) { return; }
            if (!DA.GetData<double>(1, ref fvrd)) { return; }
            if (!DA.GetData<double>(2, ref d)) { return; }
            if (!DA.GetData<double>(3, ref alpha)) { return; }
            if (!DA.GetData<double>(4, ref npar)) { return; }
            if (!DA.GetData<double>(5, ref npep)) { return; }
            if (!DA.GetData<string>(6, ref type)) { return; }
            if (!DA.GetData<bool>(7, ref method)) { return; }
            if (!DA.GetData<double>(8, ref a1)) { return; }
            if (type == "dowel") { type = "bolt"; }
            double n = npar * npep;
            double nef = Nef(d, a1, type, npar, npep);
            double nalfa = (alpha / (Math.PI / 2)) * (n - nef) + nef;
            double Util = 0;
            double FVrd = nalfa * fvrd;
            if (method)
            {
                FVrd = FVrd / n;
            }
            Util = vsd / FVrd;
            DA.SetData(0, FVrd);
            DA.SetData(1, Util);
            DA.SetData(2, nef);
        }



        public double Nef(double d, double a1, string type,double npar, double npep )
        {
            double nef = 0;
            if (type == "nail" || (type == "screw" & d < 6))
            {
                
                double kef = 0;
                if (a1 >= 4 * d & a1 < 7 * d)
                {
                    kef = 0.5 - (0.5 - 0.7) * (4 * d - a1) / (4 * d - 7 * d);
                }
                if (a1 >= 7 * d & a1 < 10 * d)
                {
                    kef = 0.7 - (0.7 - 0.85) * (7 * d - a1) / (7 * d - 10 * d);
                }
                if (a1 >= 10 * d & a1 < 14 * d)
                {
                    kef = 0.85 - (0.85 - 1) * (10 * d - a1) / (10 * d - 14 * d);
                }
                if (a1 >= 14 * d)
                {
                    kef = 1;
                }
                nef = (Math.Pow(npar, kef)) * npep;
            }
            if (type == "bolt" || (type == "screw" & d > 6))
            {
                if (npar * npep == 1) { nef = 1; }
                else
                {
                    nef = Math.Min(npar*npep, Math.Pow(npar, 0.9) * Math.Pow(a1 / (13 * d), 0.25)) * npep;
                }
            }
            return nef;
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
                return Properties.Resources._3_6;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("931793f9-ce3c-45dd-832e-e7f58c143f71"); }
        }
    }
}