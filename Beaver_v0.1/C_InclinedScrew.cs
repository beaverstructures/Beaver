using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.IO;
using System.Drawing;

namespace Beaver_v0._1
{
    public class C_InclinedScrew : GH_Component
    {
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        /// <summary>
        /// Initializes a new instance of the InclinedScrew class.
        /// </summary>
        public C_InclinedScrew()
          : base("Inclined Screws Connection", "Inclined Screw",
              "Analysis of axially loaded screw in timber-to-timber or steel-to-timber connections",
              "Beaver", "Connections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Screw Lenght (mm)", "lscrew", "Screw Lenght", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Angle screw to the Grain [rad]", "αgrain", "Screw angle related to the grain [0 to Pi/2 rad]", GH_ParamAccess.item, Math.PI / 2);
            pManager.AddNumberParameter("Screw diameter [mm]", "d", "Screw nominal diameter", GH_ParamAccess.item, 7);
            pManager.AddBooleanParameter("Steel to timber", "S2T", "True if timber to steel, false if timber to timber", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Headside depth [mm]", "th", "Headside timber depth. If S2T is true, it corresponds to the steel thickness", GH_ParamAccess.item, 20);
            pManager.AddTextParameter("WoodType", "wtype", "", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Modification Factor", "Kmod", "Modification Factor for Load Duration and Moisture Content", GH_ParamAccess.item, 0.6);
            pManager.AddNumberParameter("Screw shank diameter [mm]", "ds", "Screw shank diameter", GH_ParamAccess.item, 4.6);
            pManager.AddNumberParameter("Screw fyk [N/mm²]", "fyk", "Characteristic Yield Strength of the Screw's steel [N/mm²]", GH_ParamAccess.item, 260);


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.Register_DoubleParam("Tension resistance [N]", "Ftrd", "Tensioned axially loaded screw load capacity");
            pManager.Register_DoubleParam("Compression resistance [N]", "Fcrd", "Compressed axially loaded screw load capacity");
        }

        public override void AddedToDocument(GH_Document document)
        {
            Material timber = new Material();
            List<string> names = timber.GetTypesNames();
            if (Params.Input[5].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate new value list
                var vl = new Grasshopper.Kernel.Special.GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "Type";
                //clear default contents
                vl.ListItems.Clear();
                foreach (string name in names)
                {
                    vl.ListItems.Add(new GH_ValueListItem(name, "\"" + name + "\""));
                }

                document.AddObject(vl, false);
                Params.Input[5].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[5].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
            }
        }
        /// <su
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double alfa = 0;
            double d = 0;
            double ds = 0;
            double lscrew = 0;
            string wood = "";
            double pk;
            double kmod = 0;
            double tp = 0;
            double th = 0;
            double Rtens = 0;
            double Ym = 0;
            bool s2t = false;

            if (!DA.GetData<double>(0, ref lscrew)) { return; }
            if (!DA.GetData<double>(1, ref alfa)) { return; }
            if (!DA.GetData<double>(2, ref d)) { return; }
            if (!DA.GetData<bool>(3, ref s2t)) { return; }
            if (!DA.GetData<double>(4, ref th)) { return; }
            if (!DA.GetData<string>(5, ref wood)) { return; }
            if (!DA.GetData<double>(6, ref kmod)) { return; }
            if (!DA.GetData<double>(7, ref ds)) { return; }
            if (!DA.GetData<double>(8, ref Rtens)) { return; }

            Material timber = new Material(wood);
            pk = timber.pk;
            Ym = timber.Ym;
            string woodtype = timber.name;
            Rtens = Rtens * Math.PI * Math.Pow(ds, 2) / 4;
            //CALCULO DE Sg
            double Sg;

            if (s2t)
            {
                Sg = lscrew - (th / Math.Sin(alfa)) - 15;
            }
            else
            {
                if (th <= (lscrew-th))
                {
                    Sg = th - 15;
                }
                else
                {
                    Sg = (lscrew - th) - 15;
                }
            }

            //CALCULO DE Ralrk

            double faxk = 0.52 * Math.Pow(d, -0.5) * Math.Pow(Sg, -0.1) * Math.Pow(pk, 0.8);
            double Raxrk = faxk * d * Sg * Math.Pow(pk / 350, 0.8) / (1.2 * Math.Pow(Math.Cos(alfa), 2) + Math.Pow(Math.Sin(alfa), 2));

            //CALCULO DE RTrk
            double RTrd = Math.Min(Raxrk * kmod / 1.3, Rtens / 1.25);

            //CALCULO DE RCrk
            double RCrd = Math.Min(Raxrk * kmod / 1.3, 0.8*Rtens / 1.25);

            DA.SetData(0, RTrd);
            DA.SetData(1, RCrd);



        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources._3_10;
                // return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fa9b10f0-7ef1-4bc9-a379-bcf0b6f7fc8d"); }
        }
    }
}