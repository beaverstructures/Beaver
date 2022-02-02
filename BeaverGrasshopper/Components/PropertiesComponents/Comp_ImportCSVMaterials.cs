using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.IO;
using BeaverCore.Materials;
using System.Collections.Generic;
using System.Linq;


namespace BeaverGrasshopper.Components.PropertiesComponents
{
    public class Comp_ImportCSVMaterials : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Comp_ImportCSVMaterials class.
        /// </summary>
        public Comp_ImportCSVMaterials()
          : base("ImportCSVMaterials", "ImportCSVMat",
              "Imports a .csv file containing material data with headers, organized in the following order: " +
                "name, type, fmk, ft0k, ft90k, fc0k, fc90k, fvk, E0mean, E005, E90mean, Gmean, G05, pk, Ym. See 'csv' file provided in food4rhino.com",
              "Beaver", "External")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //0
            pManager.AddTextParameter("File path", "path", "File path ending in .csv extension", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Material(), "Imported Materials", "Mats", "Converted .csv file into Beaver Timber materials", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Material> gh_mats = new List<GH_Material>();
            string filepath = "";
            DA.GetData(0, ref filepath);
            var reader = new StreamReader(File.OpenRead(filepath));

            bool first = true;
            while (!reader.EndOfStream)
            {
                var values = reader.ReadLine().Split(',');
                
                // ignores csv header
                if (first)
                {
                    first = false;
                    continue;
                }
                string name = values[0];
                string type = values[1];
                double fmk = Double.Parse(values[2])*1e6;
                double ft0k = Double.Parse(values[3])*1e6;
                double ft90k = Double.Parse(values[4]) * 1e6;
                double fc0k = Double.Parse(values[5]) * 1e6;
                double fc90k = Double.Parse(values[6]) * 1e6;
                double fvk = Double.Parse(values[7]) * 1e6;
                double frk = Double.Parse(values[8]) * 1e6;
                double E0mean = Double.Parse(values[9]) * 1e6;
                double E0k = Double.Parse(values[10]) * 1e6;
                double E90mean = Double.Parse(values[11]) * 1e6;
                double E90k = Double.Parse(values[12]) * 1e6;
                double Gmean = Double.Parse(values[13]) * 1e6;
                double Gk = Double.Parse(values[14]) * 1e6;
                double pk = 100 * Double.Parse(values[15]) * 1e6;
                double pmean = 100 * Double.Parse(values[16]) * 1e6;
                double Ym = Double.Parse(values[17]) * 1e6;
                Material mat = new Material(
                    name, type,
                    fmk, ft0k, ft90k,
                    fc0k, fc90k, fvk,
                    E0mean, E0k, E90k,
                    Gmean, Gk,
                    pk, Ym);
                gh_mats.Add(new GH_Material(mat));
            }
            DA.SetDataList(0, gh_mats);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.ImportMaterial;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2d6a3a62-5028-4e7c-b7d0-9e1ac345cde4"); }
        }
    }
}