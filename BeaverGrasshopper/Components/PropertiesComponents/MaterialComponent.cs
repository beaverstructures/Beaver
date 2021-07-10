using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BeaverGrasshopper.Components.Connections
{
    public class MyComponent1 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MyComponent1()
          : base("Material", "Mat",
              "Create a timber material",
              "Beaver", "1. Properties")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Material Name. Perferably use material class e.g. C24 or GL24c", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "Type", "Input a text with Material type according to EC5 Table 3.2. Acceptable values: \nSolid Timber \nGluelam \nLVL", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Resistance", "fmk", "Bending Characteristic Resistance in [kN/cm²]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Parallel Tension Resistence", "ft0k", "Parallel Characteristic Tension Resistance in [kN/cm²]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Perpendicular Tension Resistence", "ft90k", "Perpendicular Characteristic Tension Resistance in [kN/cm²]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Palallel Compression Resistence", "fc0k", "Palallel Characteristic Compression Resistance in [kN/cm²]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Perpendicular Tension Resistence", "fc90k", "Perpendicular Characteristic Compression Resistance in [kN/cm²]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Shear Resistence", "fvk", "Characteristic Shear Resistance in [kN/cm²]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mean modulus of elasticity parallel", "E0mean", "Mean modulus of elasticity parallel to grain [GPa]", GH_ParamAccess.list);
            pManager.AddNumberParameter("5% modulus of elasticity parallel", "E05", "5% modulus of elasticity parallel to grain [GPa]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mean modulus of elasticity perpendicular", "E90mean", "Mean modulus of elasticity perpendiculat to grain [GPa]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mean modulus of elasticity shear", "E0mean", "Mean modulus of elasticity shear to grain [GPa]", GH_ParamAccess.list);
            pManager.AddNumberParameter("Material Coefficient", "γm", "Material Coefficient. If no value is provided, Beaver will calculate it according to EC5, 2.4.1, Tab. 2.3.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("dd050810-1ca7-4061-a72a-1a9916f29f35"); }
        }
    }
}