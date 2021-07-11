using BeaverCore.Actions;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BeaverGrasshopper { 
    public class Comp_Displacement : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Comp_Displacement class.
        /// </summary>
        public Comp_Displacement()
          : base("Comp_Displacement", "Nickname",
              "Description",
              "Beaver", "1.Frame")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacement x", "ux", "Displacement in x direction", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Displacement y", "uy", "Displacement in y direction", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Displacement z", "uz", "Displacement in z direction", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("LoadCase type", "type", "Type of corresponding loadcase", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Displacement(), "Displacement", "Disp.", "Nodal Displacement", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double u_x = 0;
            double u_y = 0;
            double u_z = 0;
            string type = "";
            DA.GetData(0, ref u_x);
            DA.GetData(1, ref u_y);
            DA.GetData(2, ref u_z);
            DA.GetData(3, ref type);
            Displacement displacement = new Displacement(u_x, u_y, u_z, type);

            DA.SetData(0, new GH_Displacement(displacement));
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
                return Properties.Resources.Screw;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4bb3bac8-42b6-4683-b698-1db7f9d0338f"); }
        }
    }
}