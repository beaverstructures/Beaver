using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.PlugIns;

using BeaverGrasshopper.Components.Utilities;

namespace BeaverGrasshopper.Components.Utilities
{
    public class GetBeaverLicense : GH_Component
    {
        public GetBeaverLicense()
          : base("GetBeaverLicense", "Beaver",
              "Calls GetLicense using CloudZoo licenses",
              "Beaver", "License")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Button","Button","Input a boolean button here for connecting to Cloud Zoo server and retrieving your license.", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "Status", "Text value indicating whether your license is valid", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool check = false;
            string info = "";
            DA.GetData(0, ref check);
            if (check)
            {
                try
                {

                    PlugInInfo beaver = PlugIn.GetPlugInInfo(new Guid("0c4aa986-29fd-4e69-860e-138e87613538"));
                    if (beaver.IsLoaded)
                    {
                        info = "Licensing verification was successful.\n" +
                            "Licenser: " + beaver.Organization +
                            "\nVersion:" + beaver.Version;
                    }
                    else
                    {
                        PlugIn.LoadPlugIn(new Guid("0c4aa986-29fd-4e69-860e-138e87613538"));
                        ExpireSolution(true);
                        info = "Your license was not verified.\n" +
                            "Please contact us at beaver.structures@gmail.com for registering a temporary free license.";
                    }
                }
                catch
                {
                    info = "Rhino Plugin BeaverLicense was not found. It can be downloaded via the PackageManager in Rhino";
                }
            }
            DA.SetData(0, info);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.BeaverIcon;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("09035dfe-a43a-4a1f-a1d1-ec3709641d0b"); }
        }
    }
}
