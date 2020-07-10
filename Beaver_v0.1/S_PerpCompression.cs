using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.Drawing;
using System.IO;

namespace Beaver_v0._1
{
    public class S_PerpCompression : GH_Component
    {
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public S_PerpCompression()
          : base("Compression at an Angle to the Grain", "Angular Compression",
             "Verifies the cross section of a beam subjected to compression forces at an angle to the grain according to Eurocode 5",
              "Beaver", "Sections")
        {
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Fcad", "Fcad", "Compression force at an Angle to the Grain [kN]", GH_ParamAccess.item, 0);     //0
            pManager.AddNumberParameter("αFcad", "αFcad", "If Fcad is applied perpendicularly, consider αFcad = 90 degrees. In cases which Fcad is applied to an angled cutted edge, consider αFcad from 0 to 90 degrees", GH_ParamAccess.item, 90); //1
            pManager.AddNumberParameter("lFcad", "lFcad", "Parallel to the grain distance where Fcad is distributed [cm]", GH_ParamAccess.item, 10);//2
            pManager.AddNumberParameter("l-left", "l-left", "Parallel to the grain distance from Fcad to the element's end or another Perpendicular Compression Force [cm]. This will influence on the effective contact area.", GH_ParamAccess.item, 0);//3
            pManager.AddNumberParameter("l-right", "l-right", "Parallel to the grain distance from Fcad to the element's end or another Perpendicular Compression Force [cm]. This will influence on the effective contact area.", GH_ParamAccess.item, 0);//4
            pManager.AddNumberParameter("l1", "l1", "Shortest Parallel to the grain distance from Fcad to another Perpendicular Compression Force [cm]. This will influence on the Kc90 adopted", GH_ParamAccess.item, 0);//5
            pManager.AddNumberParameter("Base", "b", "Section Base [cm]", GH_ParamAccess.item, 10);//6
            pManager.AddNumberParameter("Heigth", "h", "Section Heigth [cm]", GH_ParamAccess.item, 10);//7
            pManager.AddNumberParameter("Modification Factor", "Kmod", "Modification Factorbfor Duration of Load andd Moisture Content", GH_ParamAccess.item, 0.6);//8
            pManager.AddTextParameter("Material", "Material", "Section Material", GH_ParamAccess.item);//9



        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Frad", "Frad", "Resistence [kN]");
            pManager.Register_DoubleParam("DIV", "DIV", "Reason between Stress and Strength");
        }

        public override void AddedToDocument(GH_Document document)
        {
            Material timber = new Material();
            List<string> names = timber.GetTypesNames();
            if (Params.Input[10].SourceCount == 0)
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
                Params.Input[10].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[10].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
            }
            base.AddedToDocument(document);
        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs 
            double Fcad = 0;
            double acomp = 0;
            double lFcad = 0;
            double d1 = 0;
            double d2 = 0;
            double l1 = 0;
            double b = 0;
            double h = 0;
            double Kmod = 0;
            double Ym = 0;
            double fc0k = 0;
            double fc90k = 0;
            double E05 = 0;
            string test = "";
            if (!DA.GetData<double>(0, ref Fcad)) { return; }
            if (!DA.GetData<double>(1, ref acomp)) { return; }
            if (!DA.GetData<double>(2, ref lFcad)) { return; }
            if (!DA.GetData<double>(3, ref d1)) { return; }
            if (!DA.GetData<double>(4, ref d2)) { return; }
            if (!DA.GetData<double>(5, ref l1)) { return; }
            if (!DA.GetData<double>(6, ref b)) { return; }
            if (!DA.GetData<double>(7, ref h)) { return; }
            if (!DA.GetData<double>(8, ref Kmod)) { return; }
            if (!DA.GetData<string>(9, ref test)) { return; }
            Material timber = new Material(test);
            Ym = timber.Ym;
            fc0k = timber.fc0k;
            fc90k = timber.fc90k;
            E05 = timber.E005;
            int cont = -1;
            bool stop = false;
            double tipodemadeira = 0;
            if (timber.name != "GLULAM") { tipodemadeira = 1; }
            

            //Definição de valores do material 
            double fc0d = Kmod * fc0k / Ym;
            double fc90d = Kmod * fc90k / Ym;

            //compressão perpendicular ou em ângulo (garantir que acomp foi fornecido corretamente)
            if (acomp <= 90)
            {
                //Definição de valores geométricos e de tensão efetiva
                double d1min = Math.Min(d1 / 2, lFcad);
                double d1ef = Math.Min(d1min, 3);
                double d2min = Math.Min(d2 / 2, lFcad);
                double d2ef = Math.Min(d2min, 3);
                double lef = lFcad + d1ef + d2ef;
                double Aef = lef * b;
                double sigc90d = Fcad / Aef;
                double kc90 = 1;

                // determinando valor de kc90 (caso não seja majorado por um if abaixo, deve valer igual a 1.0)
                // ifs também perguntam se a madeira é MLC ou SOLID (coluna 13 do excel deve ser preenchida)

                if (l1 >= 2 * h)
                {
                    if (tipodemadeira == 1 && lFcad <= 40)
                    {
                        kc90 = 1.75;
                    }
                    if (tipodemadeira == 0)
                    {
                        kc90 = 1.5;
                    }
                }



                //Verificação de compressão perpendicular ou em ângulo

                double acompR = Math.PI * acomp / 180;
                double DIV = sigc90d * (fc0d * Math.Pow(Math.Sin(acompR), 2) / (kc90 * fc90d) + Math.Pow(Math.Cos(acompR), 2)) / fc0d;
                double fcad = sigc90d * Aef / DIV;
                DA.SetData(0, fcad);
                DA.SetData(1, DIV);


            }

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
                return Properties.Resources._2_3;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fa016e35-fc46-453c-917e-090443e35b19"); }
        }
    }
}