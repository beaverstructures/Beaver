using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Karamba.Materials;
using Karamba.GHopper.Materials;
using BeaverCore.Materials;

namespace BeaverGrasshopper.Components.InteropComponents
{
    public class BeaverToKarambaMaterial : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BeaverToKarambaMaterial class.
        /// </summary>
        public BeaverToKarambaMaterial()
          : base("BeaverToKarambaMaterial", "B2Kmat",
              "Converts a Beaver Material into a Karamba Material",
              "Beaver", "External")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Material(), "Beaver Material", "BMat",
                        "Beaver Timber Material", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_FemMaterial(), "Karamba Material", "K3DMat",
                        "Converted Karamba Timber Material", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Material ghBeaverMaterial = new GH_Material();
            DA.GetData(0, ref ghBeaverMaterial);
            Material beaverMaterial = ghBeaverMaterial.Value;

            string family = beaverMaterial.type;
            string name = beaverMaterial.name;
            double E1 = beaverMaterial.E0mean* 1e-3;     // Pa to kN/m²
            double E2 = beaverMaterial.E90mean * 1e-3;
            double G12 = beaverMaterial.Gmean * 1e-3;
            double nue12 = -1; // !!! change later
            double G31 = beaverMaterial.G05 * 1e-3;
            double G32 = beaverMaterial.G05 * 1e-3;
            double gamma = beaverMaterial.pk * 1e-3;    // N/m³ to kN/m³
            double ft1 = beaverMaterial.ft0k * 1e-3;
            double ft2 = beaverMaterial.ft90k * 1e-3;
            double fc1 = -beaverMaterial.fc0k * 1e-3;
            double fc2 = -beaverMaterial.fc90k * 1e-3;
            double t12 = beaverMaterial.fvk * 1e-3;
            double f12 = beaverMaterial.fvk * 1e-3;
            

            double alphaT1 = 5E-6; // !!! change later
            double alphaT2 = 5E-6; // !!! change later

            FemMaterial_Orthotropic karambaMaterial = new FemMaterial_Orthotropic(
                family, name, E1, E2, 
                G12, nue12, G31, G32, 
                gamma, ft1, ft2, fc1, fc2, t12, f12,
                FemMaterial.FlowHypothesisFromString("RANKINE"), alphaT1, 
                alphaT2, null);

            karambaMaterial.UserData["fmk"] = beaverMaterial.fmk * 1e-3;    // Pa to kN/m²
            karambaMaterial.UserData["fvk"] = beaverMaterial.fvk * 1e-3;
            karambaMaterial.UserData["frk"] = beaverMaterial.frk * 1e-3;

            karambaMaterial.UserData["pmean"] = beaverMaterial.pmean * 1e-3 ; // N/m³ to kN/m³

            karambaMaterial.UserData["ym"] = beaverMaterial.Ym;
            karambaMaterial.UserData["Bc"] = beaverMaterial.Bc;
            karambaMaterial.UserData["kdef"] = beaverMaterial.kdef;

            karambaMaterial.UserData["E05"] = beaverMaterial.E05 * 1e-3;
            karambaMaterial.UserData["E90_05"] = beaverMaterial.E90_05 * 1e-3;


            DA.SetData(0, new GH_FemMaterial(karambaMaterial));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.ExportMaterialtoKaramba;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("df67861f-a0ee-404a-8de8-b88c50e52bea"); }
        }
    }
}