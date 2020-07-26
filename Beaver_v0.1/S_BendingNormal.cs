using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.IO;
using System.Drawing;

namespace Beaver_v0._1
{
    public class S_BendingNormal : GH_Component
    {
        GH_Document GrasshopperDocument;
        IGH_Component Component;
        

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public S_BendingNormal()
          : base("Bending and Normal Forces - General", "Bending & Normal",
             "Verifies the cross section of a beam subjected to Bending and Normal forces according to Eurocode 5",
              "Beaver", "Sections")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Nd", "Nd", "Normal Forces Parallel to the Grain [kN]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Myd", "Myd", "Y local axis Bending Forces [kNm]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Mzd", "Mzd", "Z local axis Bending Forces [kNm]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Base", "b", "Section Base [cm]", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Heigth", "h", "Section Heigth [cm]", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Length", "l", "Element Length [m]", GH_ParamAccess.item, 4);
            pManager.AddNumberParameter("Effective Length as a Ratio of the Span", "kflam", "lef/l", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Modification Factor", "Kmod", "Modification Factor for Load Duration and Moisture Content", GH_ParamAccess.item, 0.6);
            pManager.AddTextParameter("Material", "Material", "Section Material", GH_ParamAccess.item, "");
            //pManager.AddNumberParameter("Km", "Km", "Use 0.7 for Rectangular Cross Sections (Glulam and LVL only) and 1.0 for Other", GH_ParamAccess.item,0.7);
            // pManager.AddNumberParameter("Gama m", "Ym", "Coeficiente de redução do material", GH_ParamAccess.item);
            // pManager.AddNumberParameter("Resistência à Compressão", "fc0k", "Resistência à Compressão paralela à fibra [KN/cm2]", GH_ParamAccess.item);
            // pManager.AddNumberParameter("Resistência à Flexão", "fmk", "Resistência à Flexão [KN/cm2]", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Módulo de Elasticidade", "E05", "Módulo de Young do material, que caracteriza sua rigidez à um escoamento de 5% [KN/cm2]", GH_ParamAccess.item);
            // pManager.AddNumberParameter("Beta c", "Bc", "Coeficiente influente na analise de pilares, 0.1 para MLC e 0.2 para solido", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("UtilY", "UtilY", "Reason between Stress and Strength appliying Km on the Z axis");
            pManager.Register_DoubleParam("UtilZ", "UtilZ", "Reason between Stress and Strength appliying Km on the Y axis");
            pManager.Register_DoubleParam("Relative Lambda", "lamm", "For the stability analysis, an element will be considered as a Column if (lamm < 0.75) and a Beam if (lamm > 0.75)");
        }

        public override void AddedToDocument(GH_Document document)
        {
            Material timber = new Material();
            List<string> names = timber.GetTypesNames();
            if (Params.Input[8].SourceCount == 0)
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
                Params.Input[8].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[8].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
            }
            base.AddedToDocument(document);
        }

            
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            //inputs 
            double Nd = 0;
            double Myd = 0;
            double Mzd = 0;
            double b = 0;
            double h = 0;
            double l = 0;
            double kflam = 0;
            double Kmod = 0;
            double Km = 0.7;
            double Ym = 0;
            double fc0k = 0;
            double ft0k = 0;
            double fmk = 0;
            double E05 = 0;
            double Bc = 0.2;
            string test = "";
            if (!DA.GetData<double>(0, ref Nd)) { return; }
            if (!DA.GetData<double>(1, ref Myd)) { return; }
            if (!DA.GetData<double>(2, ref Mzd)) { return; }
            if (!DA.GetData<double>(3, ref b)) { return; }
            if (!DA.GetData<double>(4, ref h)) { return; }
            if (!DA.GetData<double>(5, ref l)) { return; }
            if (!DA.GetData<double>(6, ref kflam)) { return; }
            if (!DA.GetData<string>(8, ref test)) { return; }
            if (!DA.GetData<double>(7, ref Kmod)) { return; }

            if (test=="") { test = "GL 24h"; }
            Material timber = new Material(test);
            Ym = timber.Ym;
            fc0k = timber.fc0k;
            ft0k = timber.ft0k;
            fmk = timber.fmk;
            E05 = timber.E005;
            if (timber.name == "GLULAM")
            {
                Bc = 0.1;
            }
            else { Bc = 0.2; }
           
            //Definição de valores geométricos
            double A = h * b;
            double Iy = b * Math.Pow(h, 3) / 12;
            double Iz = h * Math.Pow(b, 3) / 12;
            double Wy = Iy * (2 / h);
            double Wz = Iz * (2 / b);
            double ry = Math.Sqrt(Iy / A);
            double rz = Math.Sqrt(Iz / A);
            double lamy = (100 * l)/ry;
            double lamz = (100 * l)/rz;
            double lef = kflam * l * 100;

            //Definição de valores do material (ver se os sigmas devem mesmo serem multiplicados por 100)
            double lampi = Math.Sqrt(fc0k / E05) / Math.PI;
            double fc0d = Kmod * fc0k / Ym;
            double ft0d = Kmod * ft0k / Ym;
            double fmd = Kmod * fmk / Ym;
            double lamyrel = lamy * lampi;
            double lamzrel = lamz * lampi;
            double sigN = Nd / A;
            double sigMy = 100 * Myd / Wy;
            double sigMz = 100 * Mzd / Wz;
            double G05 = E05 / 16;
            double UtilY = 0;
            double UtilZ = 0;
            double UtilYg = 0;
            double UtilZg = 0;
            double UtilY = 0;
            double UtilZ = 0;

            //Definição dos valores de cálculo necessários para verificação em pilares ou vigas (exclui parte em que dizia que lamrely=lamm e lamrelz=sgmcrit)
            double sigMcrit = (0.78*Math.Pow(b,2)/(h*lef))*E05;
            double lamm = Math.Sqrt(fmk / sigMcrit);
            double ky = 0.5 * (1 + Bc * (lamyrel - 0.3) + Math.Pow(lamyrel, 2));
            double kz = 0.5 * (1 + Bc * (lamzrel - 0.3) + Math.Pow(lamzrel, 2));
            double kyc = 1 / (ky + Math.Sqrt(Math.Pow(ky, 2) - Math.Pow(lamyrel, 2)));
            double kzc = 1 / (kz + Math.Sqrt(Math.Pow(kz, 2) - Math.Pow(lamzrel, 2)));
            if (Nd < 0)
            {
                Nd = Math.Abs(Nd);
                sigN = Math.Abs(sigN);
                //Verificação de comportamento de Pilares
                if (lamm < 0.75)
                {
                    if (lamyrel <= 0.3 && lamzrel <= 0.3)
                    {
                        UtilY = Math.Pow(sigN / fc0d, 2) + (sigMy / fmd) + Km * (sigMz / fmd);
                        UtilZ = Math.Pow(sigN / fc0d, 2) + Km * (sigMy / fmd) + (sigMz / fmd);

                    }
                    else
                    {
                        List<double> Utils = new List<double>();
                        UtilY = (sigN / (kyc * fc0d)) + (sigMy / fmd) + Km * (sigMz / fmd);
                        UtilZ = (sigN / (kzc * fc0d)) + Km * (sigMy / fmd) + (sigMz / fmd);

                    }
                }
                //Verificação de comportamento de Vigas
                else
                {

                    if (lamm >= 0.75 && lamm < 1.4)
                    {
                        double kcrit = 1.56 - 0.75 * lamm;
                        UtilY = Math.Pow(sigMy / (kcrit * fmd), 2) + (sigN / (kzc * fc0d)) + Km * (sigMz / fmd);
                        UtilZ = 0;

                    }
                    if (lamm >= 1.4)
                    {
                        double kcrit = 1 / Math.Pow(lamm, 2);
                        UtilY = Math.Pow(sigMy / (kcrit * fmd), 2) + (sigN / (kzc * fc0d)) + Km * (sigMz / fmd);
                        UtilZ = 0;

                    }
                }


                
            }
            else if (Nd >= 0)
            {
                if (lamm >= 0.75 && lamm < 1.4)
                {
                    double kcrit = 1.56 - 0.75 * lamm;
                    UtilY = Math.Pow(sigMy / (kcrit * fmd), 2) + (sigN / ft0d) + Km * (sigMz / fmd);
                    UtilZ = Km*(sigMy / fmd) + (sigN / ft0d) + (sigMz / fmd);

                }
                if (lamm >= 1.4)
                {
                    double kcrit = 1 / Math.Pow(lamm, 2);
                    UtilY = Math.Pow(sigMy / (kcrit * fmd), 2) + (sigN / (ft0d)) + Km * (sigMz / fmd);
                    UtilZ = Km*(sigMy / (kcrit * fmd)) + (sigN / ft0d) + (sigMz / fmd);

                }
                else
                {
                    UtilY = (sigN / ft0d) + (sigMy / fmd) + Km * (sigMz / fmd);
                    UtilZ = (sigN / ft0d) + Km * (sigMy / fmd) + (sigMz / fmd);
                }
            }
            
            UtilY = UtilY;
            UtilZ = UtilZ;
            DA.SetData(0, UtilY);
            DA.SetData(1, UtilZ);
            DA.SetData(2, lamm);


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
                return Properties.Resources._2_4;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("019a3f0d-52c4-4ad4-8fdd-02b1b23249a7"); }
        }
    }
}