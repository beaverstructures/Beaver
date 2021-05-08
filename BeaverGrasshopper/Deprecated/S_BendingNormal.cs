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
          : base("Bending and Normal Forces", "Bending & Normal",
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
            pManager.AddNumberParameter("Buckling Length Y", "BklLenY", "Effective Bucking Length of Element in local Y direction [m] if > 0", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Buckling Length Z", "BklLenZ", "Effective Bucking Length of Element in local Z direction [m] if > 0", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Effective Length as a Ratio of the Span", "kflam", "lef/l (EC5 Table 6.1)", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Modification Factor", "Kmod", "Modification Factor for Load Duration and Moisture Content", GH_ParamAccess.item, 0.6);
            pManager.AddTextParameter("Material", "Material", "Section Material", GH_ParamAccess.item, "");

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("UtilY", "UtilY", "Reason between Stress and Strength appliying Km on the Z axis");
            pManager.Register_DoubleParam("UtilZ", "UtilZ", "Reason between Stress and Strength appliying Km on the Y axis");
            pManager.Register_StringParam("info", "info", "Main calculated parameters");
        }

        public override void AddedToDocument(GH_Document document)
        {
            Material timber = new Material();
            List<string> names = timber.GetTypesNames();
            if (Params.Input[9].SourceCount == 0)
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
                Params.Input[9].AddSource(vl);
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
            double ly = 0;
            double lz = 0;
            double Kmod = 0;
            double Km = 1;
            double Ym = 0;
            double fc0k = 0;
            double ft0k = 0;
            double fmk = 0;
            double E05 = 0;
            double kflam = 0;
            double Bc = 0.2;
            
            string test = "";
            if (!DA.GetData<double>(0, ref Nd)) { return; }
            if (!DA.GetData<double>(1, ref Myd)) { return; }
            if (!DA.GetData<double>(2, ref Mzd)) { return; }
            if (!DA.GetData<double>(3, ref b)) { return; }
            if (!DA.GetData<double>(4, ref h)) { return; }
            if (!DA.GetData<double>(5, ref ly)) { return; }
            if (!DA.GetData<double>(6, ref lz)) { return; }
            if (!DA.GetData<double>(7, ref kflam)) { return; }
            if (!DA.GetData<double>(8, ref Kmod)) { return; }
            if (!DA.GetData<string>(9, ref test)) { return; }


            if (test=="") { test = "GL 24h"; }
            Material timber = new Material(test);
            Ym = timber.Ym;
            fc0k = timber.fc0k;
            ft0k = timber.ft0k;
            fmk = timber.fmk;
            E05 = timber.E005;
            if (timber.name == "GLULAM" || timber.name == "LVL")
            {
                Bc = 0.1;
                Km = 0.7;
            }
            else { Bc = 0.2; Km = 1; }
           
            //Definição de valores geométricos
            double A = h * b;
            double Iy = b * Math.Pow(h, 3) / 12;
            double Iz = h * Math.Pow(b, 3) / 12;
            double Wy = Iy * (2 / h);
            double Wz = Iz * (2 / b);
            double ry = Math.Sqrt(Iy / A);
            double rz = Math.Sqrt(Iz / A);
            double lamy = (100 * ly)/ry;
            double lamz = (100 * lz)/rz;
            double lefy = ly * kflam * 100;
            double lefz = lz * kflam * 100;

            //Definição de valores do material 
            double lampi = Math.Sqrt(fc0k / E05) / Math.PI;
            double fc0d = Kmod * fc0k / Ym;
            double ft0d = Kmod * ft0k / Ym;
            double fmd = Kmod * fmk / Ym;
            double lamyrel = lamy * lampi;
            double lamzrel = lamz * lampi;
            double sigN = Nd / A;
            double sigMy = 100 * Math.Abs(Myd) / Wy;
            double sigMz = 100 * Math.Abs(Mzd) / Wz;
            double G05 = E05 / 16;
            double UtilY = 0;
            double UtilZ = 0;
            string info = "";

            //Definição dos valores de cálculo necessários para verificação em pilares ou vigas 
            double sigMcrity = (0.78*Math.Pow(b,2)/(h*lefy))*E05;
            double sigMcritz = (0.78 * Math.Pow(h, 2) / (b * lefz)) * E05;
            double lammy = Math.Sqrt(fmk / sigMcrity);
            double lammz = Math.Sqrt(fmk / sigMcritz);
            double ky = 0.5 * (1 + Bc * (lamyrel - 0.3) + Math.Pow(lamyrel, 2));
            double kz = 0.5 * (1 + Bc * (lamzrel - 0.3) + Math.Pow(lamzrel, 2));
            double kyc = 1 / (ky + Math.Sqrt(Math.Pow(ky, 2) - Math.Pow(lamyrel, 2)));
            double kzc = 1 / (kz + Math.Sqrt(Math.Pow(kz, 2) - Math.Pow(lamzrel, 2)));
            string loaddata = "";

            if (Nd > 0) {
                if ( Myd==0 && Mzd == 0)
                {
                    loaddata = "Pure Tension";
                }
                else
                {
                    loaddata = "Tension + Bending";
                }
            }
            else if (Nd < 0)
            {
                if (Myd == 0 && Mzd == 0)
                {
                    loaddata = "Pure Compression";
                }
                else
                {
                    loaddata = "Compression + Bending";
                }
            }
            else if (Nd==0 && (Myd!=0 || Mzd != 0))
            {
                loaddata = "Pure Bending";
            }
            string data = string.Format("λy ={0}, λz ={1},λrely ={2}, λrelz ={3}, ky ={4}, kz ={5}, kcy={6}, kcz={7}, σMcrity= {8}, σMcritz= {9}, λmy ={10}, λmz ={11}",
                Math.Round(lamy,3), Math.Round(lamz,3), Math.Round(lamyrel,3), Math.Round(lamzrel,3), Math.Round(ky,3), Math.Round(kz, 3),
                Math.Round(kyc,3), Math.Round(kzc, 3), Math.Round(sigMcrity, 3), Math.Round(sigMcritz,3), Math.Round(lammy,3), Math.Round(lammz,3));

            if (Nd < 0)
            {
                Nd = Math.Abs(Nd);
                sigN = Math.Abs(sigN);
                //Verificação de comportamento de Pilares
                if (Math.Max(lammy,lammz) < 0.75) //checar se é isso mesmo, ou devo considerar apenas lammy
                {
                    if (lamyrel <= 0.3 && lamzrel <= 0.3)
                    {
                        UtilY = Math.Pow(sigN / fc0d, 2) + (sigMy / fmd) + Km * (sigMz / fmd);
                        UtilZ = Math.Pow(sigN / fc0d, 2) + Km * (sigMy / fmd) + (sigMz / fmd);
                        info = loaddata + ", No Buckling effect:" + data;
                    }
                    else
                    {
                        UtilY = (sigN / (kyc * fc0d)) + (sigMy / fmd) + Km * (sigMz / fmd);
                        UtilZ = (sigN / (kzc * fc0d)) + Km * (sigMy / fmd) + (sigMz / fmd);

                    }

                    info = loaddata + ", Acts as a Column (λm<0.75): " + data;
                }
                //Verificação de comportamento de Vigas
                else
                {

                    if (Math.Max(lammy, lammz) >= 0.75 && Math.Max(lammy, lammz) < 1.4) 
                    {
                        double kcrity = Getkcrit(lammy);
                        double kcritz = Getkcrit(lammz);
                        UtilY = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / (kzc * fc0d)) + Km * (sigMz / fmd);
                        UtilZ = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / (kyc * fc0d)) + Km * (sigMy / fmd); ;
                        info = loaddata + ", Acts as a Beam (0.75 <= λm < 1.4): " + data;

                    }
                    if (Math.Max(lammy, lammz) >= 1.4)
                    {
                        double kcrity = Getkcrit(lammy);
                        double kcritz = Getkcrit(lammz);
                        UtilY = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / (kzc * fc0d)) + Km * (sigMz / fmd);
                        UtilZ = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / (kyc * fc0d)) + Km * (sigMy / fmd);
                        info = loaddata +", Acts as a Beam (λm >= 1.4): " + data;
                    }



                }


                
            }
            else if (Nd >= 0)
            {
                if (Math.Max(lammy, lammz) >= 0.75 && Math.Max(lammy, lammz) < 1.4)
                {
                    double kcrity = Getkcrit(lammy);
                    double kcritz = Getkcrit(lammz);
                    UtilY = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / ft0d) + Km * (sigMz / fmd);
                    UtilY = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / ft0d) + Km * (sigMy / fmd);
                    info = loaddata + ", Acts as a Beam (0.75 <= λm < 1.4): " + data;
                }
                else if (Math.Max(lammy, lammz) >= 1.4)
                {
                    double kcrity = Getkcrit(lammy);
                    double kcritz = Getkcrit(lammz);
                    UtilY = Math.Pow(sigMy / (kcrity * fmd), 2) + (sigN / (ft0d)) + Km * (sigMz / fmd);
                    UtilY = Math.Pow(sigMz / (kcritz * fmd), 2) + (sigN / (ft0d)) + Km * (sigMy / fmd);
                    info = loaddata + ", Acts as a Beam (λm >= 1.4): " + data;
                }
                else
                {
                    UtilY = (sigN / ft0d) + (sigMy / fmd) + Km * (sigMz / fmd);
                    UtilZ = (sigN / ft0d) + Km * (sigMy / fmd) + (sigMz / fmd);
                    info = loaddata + ", No Torsional Buckling effects, λmy=" + Math.Round(lammy,3) + ", λmyz=" + Math.Round(lammz, 3);
                }
            }

            

            DA.SetData(0, UtilY);
            DA.SetData(1, UtilZ);
            DA.SetData(2, info); 


        }
        double Getkcrit(double lam)
        {
            double kcrit = 1;
            if (lam>=0.75 && lam < 1.4)
            {
                kcrit = 1.56 - 0.75 * lam;
            }
            else if (lam >= 1.4)
            {
                kcrit = 1 / Math.Pow(lam, 2);
            }
            return kcrit;
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