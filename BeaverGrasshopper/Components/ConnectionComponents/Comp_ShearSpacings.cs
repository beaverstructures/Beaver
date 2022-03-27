using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using BeaverCore.Connections;

namespace BeaverGrasshopper.Components.ConnectionComponents
{
    public class Comp_ShearSpacings : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Comp_ShearSpacings()
          : base("Shear Connection Spacings", "Spacings",
              "Assembles the connection spacings to be used in the connection analysis",
              "Beaver", "2. Connection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Npar", "Npar", "Number of rows paralell to grain", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Nperp", "Nperp", "Number of rows perpendicular to grain ", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Stagger", "Stagger", "Boolean indicating whether the arrangement is staggered", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("a1", "a1", "Spacing parallel to the grain", GH_ParamAccess.item);
            pManager.AddNumberParameter("a2", "a2", "Spacing perpendicular to the grain", GH_ParamAccess.item);
            pManager.AddNumberParameter("a3", "a3", "end spacing", GH_ParamAccess.item);
            pManager.AddNumberParameter("a4", "a4", "edge distance", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "connection reference plane", GH_ParamAccess.item, Plane.WorldYZ);
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_ShearSpacing(), "Spacing", "Spacing", "Beaver spacing element", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int npar = 1;
            int nperp = 1;
            double a1 = 0;
            double a2 = 0;
            double a3 = 0;
            double a4 = 0;
            bool stagger = true;

            DA.GetData(0, ref npar);
            DA.GetData(1, ref nperp);
            DA.GetData(2, ref stagger);
            DA.GetData(3, ref a1);
            DA.GetData(4, ref a2);
            DA.GetData(5, ref a3);
            DA.GetData(6, ref a4);

            ShearSpacing spacing = new ShearSpacing(a1,a2,a3,a4,npar,nperp);
            DA.SetData(0, new GH_ShearSpacing(spacing));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Spacing;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ebe4a519-3743-4110-8c40-038b0fd72c55"); }
        }
    }
}