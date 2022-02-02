using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using BeaverCore.Materials;

namespace BeaverGrasshopper
{
    public class BeaverMaterial : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BeaverMateriall class.
        /// </summary>
        public BeaverMaterial()
          : base("Material", "BMat",
              "Creates a Beaver Material with properties needed to ULS checks",
              "Beaver", "Properties")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //0
            pManager.AddTextParameter("Name", "Name", "Material Name. Perferably use material class e.g. C24 or GL24c. Default is set to GL24c.", GH_ParamAccess.item, "GL24c");
            //1
            pManager.AddTextParameter("Type", "Type", "Input a text with Material type according to EC5 Table 3.2. Acceptable values: \nSolid Timber \nGluelam \nLVL. Default is set to Gluelam.", GH_ParamAccess.item, "Glulam");
            //2
            pManager.AddNumberParameter("Bending Resistance", "fmk", "Bending Characteristic Resistance in [kN/cm²]. Default is set to 2.4 kN/cm².", GH_ParamAccess.item, 2.4);
            //3
            pManager.AddNumberParameter("Parallel Tension Resistence", "ft0k", "Parallel Characteristic Tension Resistance in [kN/cm²]. Default is set to 1.65 kN/cm².", GH_ParamAccess.item, 1.65);
            //4
            pManager.AddNumberParameter("Perpendicular Tension Resistence", "ft90k", "Perpendicular Characteristic Tension Resistance in [kN/cm²]. Default is set to 0.04 kN/cm².", GH_ParamAccess.item, 0.04);
            //5
            pManager.AddNumberParameter("Palallel Compression Resistence", "fc0k", "Palallel Characteristic Compression Resistance in [kN/cm²]. Default is set to 2.40 kN/cm².", GH_ParamAccess.item, 2.4);
            //6
            pManager.AddNumberParameter("Perpendicular Compression Resistence", "fc90k", "Perpendicular Characteristic Compression Resistance in [kN/cm²]. Default is set to 0.27 kN/cm².", GH_ParamAccess.item, 0.27);
            //7
            pManager.AddNumberParameter("Shear Resistence", "fvk", "Characteristic Shear Resistance in [kN/cm²]. Default is set to 0.4 kN/cm².", GH_ParamAccess.item, 0.4);
            //8
            pManager.AddNumberParameter("Mean modulus of elasticity parallel", "E0mean", "Mean modulus of elasticity parallel to grain [GPa]. Default is set to 1160 GPa.", GH_ParamAccess.item, 1160);
            //9
            pManager.AddNumberParameter("5% modulus of elasticity parallel", "E05", "5% modulus of elasticity parallel to grain [GPa]. Default is set to 940 GPa.", GH_ParamAccess.item, 940);
            //10
            pManager.AddNumberParameter("Mean modulus of elasticity perpendicular", "E90mean", "Mean modulus of elasticity perpendiculat to grain [GPa]. Default is set to 0 GPa.", GH_ParamAccess.item, 0);
            //11
            pManager.AddNumberParameter("Mean modulus of elasticity shear", "Gmean", "Mean modulus of elasticity shear to grain [GPa]. Default is set to 58.75 GPa.", GH_ParamAccess.item, 58.75);
            //12
            pManager.AddNumberParameter("5% modulus of elasticity shear", "G05", "5th-percentile modulus of elasticity shear to grain [GPa]. Default is set to 54 GPa.", GH_ParamAccess.item, 54);
            //13
            pManager.AddNumberParameter("Material Coefficient", "γm", "Material Coefficient. If no value is provided, Beaver will calculate it according to EC5, 2.4.1, Tab. 2.3.", GH_ParamAccess.item, 1.25);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Material(), "Material", "Mat", "Beaver Timber material", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            string type = "";
            double fmk = 0;
            double ft0k = 0;
            double ft90k = 0;
            double fc0k = 0;
            double fc90k = 0;
            double fvk = 0;

            double E0mean = 0;
            double E05 = 0;

            double Gmean = 0;
            double G05 = 0;
            double E90mean = 0;

            double Ym = 0;


            DA.GetData(0, ref name);
            DA.GetData(1, ref type);

            DA.GetData(2, ref fmk);
            DA.GetData(3, ref ft0k);
            DA.GetData(4, ref ft90k);
            DA.GetData(5, ref fc0k);
            DA.GetData(6, ref fc90k);
            DA.GetData(7, ref fvk);

            DA.GetData(8, ref E0mean);
            DA.GetData(9, ref E05);

            DA.GetData(10, ref E90mean);
            DA.GetData(11, ref Gmean);
            DA.GetData(12, ref G05);

            DA.GetData(13, ref Ym);

            
            Material mat = new Material(
                name, 
                type,
                fmk * 1e7, 
                ft0k * 1e7, 
                ft90k * 1e7, 
                fc0k * 1e7, 
                fc90k * 1e7, 
                fvk * 1e7, 
                E0mean * 1e7, 
                E05 * 1e7, 
                E90mean * 1e7, 
                Gmean * 1e7, 
                G05 * 1e7, 
                Ym);

            DA.SetData(0, new GH_Material(mat));
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
                return Properties.Resources.Material;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ff7ec815-f3cd-4c53-878f-e32bc33ae53f"); }
        }
    }
}