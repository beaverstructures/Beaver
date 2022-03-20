using BeaverCore.Actions;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BeaverGrasshopper
{
    public class Comp_Force : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Comp_Force class.
        /// </summary>
        public Comp_Force()
          : base("Force", "Force",
              "Description",
              "Beaver", "1.Frame")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Normal Force", "N", "Normal force at point", GH_ParamAccess.item,0);
            pManager.AddNumberParameter("Shear Force Y", "Vy", "Shear force at point in y direction", GH_ParamAccess.item,0);
            pManager.AddNumberParameter("Shear Force Z", "Vz", "Shear force at point in z direction", GH_ParamAccess.item,0);
            pManager.AddNumberParameter("Torsional Moment", "Mt", "Normal force at point", GH_ParamAccess.item,0);
            pManager.AddNumberParameter("Bending Moment Y", "My", "Normal force at point", GH_ParamAccess.item,0);
            pManager.AddNumberParameter("Bending Moment Z", "Mz", "Normal force at point", GH_ParamAccess.item,0);
            pManager.AddTextParameter("LoadCase type", "type", "Type of corresponding loadcase", GH_ParamAccess.item,"P");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Force(), "Force", "Force", "Nodal Force", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double N = 0;
            double Vy = 0;
            double Vz = 0;
            double Mx = 0;
            double My = 0;
            double Mz = 0;
            string type = "";
            DA.GetData(0, ref N);
            DA.GetData(1, ref Vy);
            DA.GetData(2, ref Vz);
            DA.GetData(3, ref Mx);
            DA.GetData(4, ref My);
            DA.GetData(5, ref Mz);
            DA.GetData(6, ref type);
            Force force = new Force(N, Vy, Vz, Mx, My, Mz, type);

            DA.SetData(0, new GH_Force(force));
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
                return Properties.Resources.Force;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("613bac1e-22b0-4178-b449-2ca549efc10c"); }
        }
    }
}