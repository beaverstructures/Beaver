using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using BeaverCore.CrossSection;
using BeaverCore.Materials;
using Grasshopper.Kernel.Special;
using System.Drawing;

namespace BeaverGrasshopper
{
    public class Comp_CrossSection : GH_Component, IGH_VariableParameterComponent
    {

        string crosec_type = "rectangular";
        // bool updated = false;

        /// <summary>
        /// Initializes a new instance of the Comp_CrossSection class.
        /// </summary>
        public Comp_CrossSection()
          : base("CrossSection", "CS",
              "Computes a Beaver Crossection",
              "Beaver", "1. Frame")
        {
        }

        //public override void AddedToDocument(GH_Document document)
        //{
        //    if (Params.Input[0].SourceCount == 0)
        //    {
        //        // Perform Layout to get actual positionning of the component on the canevas
        //        this.Attributes.ExpireLayout();
        //        this.Attributes.PerformLayout();

        //        //instantiate new value list
        //        var vl = new Grasshopper.Kernel.Special.GH_ValueList();
        //        vl.CreateAttributes();
        //        vl.NickName = "Type";
        //        //clear default contents
        //        vl.ListItems.Clear();
        //        vl.ListItems.Add(new GH_ValueListItem("Rectangular", "\"rectangular\"" ));
        //        vl.ListItems.Add(new GH_ValueListItem("Circular", "\"circular\"" ));
        //        document.AddObject(vl, false);
        //        if (Params.Input[0].)
        //        Params.Input[0].AddSource(vl);
        //        //get the pivot of the "accent" param
        //        PointF currPivot = Params.Input[0].Attributes.Pivot;
        //        //set the pivot of the new object
        //        vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
        //    }
        //    base.AddedToDocument(document);
        //}

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type", "type", "Cross Section type", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Material(), "Material", "Mat.", "Timber Material", GH_ParamAccess.item);
            Params.Input[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_CroSec(), "Cross Section", "CroSec.", "CrossSection", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Material ghmaterial = new GH_Material();
            DA.GetData(0, ref crosec_type);
            DA.GetData(1, ref ghmaterial);
            Material material = ghmaterial.Value;
            if (crosec_type == "circular")
            {
                double d = 0;
                DA.GetData(2, ref d);
                CroSec cross_section = new CroSec_Circ(d, material);
                DA.SetData(0, new GH_CroSec(cross_section));
            }
            else if (crosec_type == "rectangular")
            {
                double h = 0;
                double b = 0;
                DA.GetData(2, ref h);
                DA.GetData(3, ref b);
                CroSec cross_section = new CroSec_Rect(h, b, material);
                DA.SetData(0, new GH_CroSec(cross_section));
            }
            else
            {
                throw new ArgumentException("Something wrong here");
            }
        }

        protected override void AfterSolveInstance()
        {
            VariableParameterMaintenance();
            Params.OnParametersChanged();
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
                return Properties.Resources.CrossSection;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("19ef7cce-39dc-445c-a681-e19989ea1a3a"); }
        }

        #region VARIABLE COMPONENT INTERFACE IMPLEMENTATION
        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {

            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {

            // Has to return a parameter object!
            Param_Number param = new Param_Number();
            string name = "";
            string nickname = "";
            string description = "";
            if (crosec_type == "rectangular")
            {
                if (index == 2)
                {
                    name = "Height";
                    nickname = "h";
                    description = "Cross Section Height";
                }
                else if (index == 3)
                {
                    name = "Width";
                    nickname = "b";
                    description = "Cross Section Width";
                }
            }
            else if (crosec_type == "circular")
            {
                name = "Diameter";
                nickname = "d";
                description = "Cross Section Diameter";
            }
            param.Name = name;
            param.NickName = nickname;
            param.Description = description;
            param.Optional = true;
            return param;


        }


        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            //This
            //nction will be called when a parameter is about to be removed. 
            //You do not need to do anything, but this would be a good time to remove 
            //any event handlers that might be attached to the parameter in question.


            return true;
        }

        public void VariableParameterMaintenance()
        {
            //This method will be called when a closely related set of variable parameter operations completes. 
            //This would be a good time to ensure all Nicknames and parameter properties are correct. This method will also be 
            //called upon IO operations such as Open, Paste, Undo and Redo.


            //throw new NotImplementedException();

            if (crosec_type == "rectangular")
            {
                if (Params.Input.Count == 4) return;
                else
                {
                    if (Params.Input.Count > 2)
                    {
                        Params.Input[2].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[2]);
                    }
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 2));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 3));
                }
            }
            else if (crosec_type == "circular")
            {
                if (Params.Input.Count == 3) return;
                else
                {
                    if (Params.Input.Count > 2)
                    {
                        Params.Input[3].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[3]);
                        Params.Input[2].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[2]);
                    }
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 2));

                }
            }



        }


        #endregion



    }
}