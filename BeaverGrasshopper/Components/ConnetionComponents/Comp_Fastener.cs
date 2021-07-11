using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using BeaverCore.Connections;
using System.Drawing;
using Grasshopper.Kernel.Parameters;

namespace BeaverGrasshopper.Components.ConnetionComponents
{
    public class Comp_Fastener : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Comp_Fastener()
          : base("Fastener", "Fast.",
              "Fastener properties",
              "Beaver", "2.Connections")
        {
        }

        string Ftype = "Dowel";

        public override void AddedToDocument(GH_Document document)
        {
            if (Params.Input[0].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate new value list
                var vl = new Grasshopper.Kernel.Special.GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "FType";
                //clear default contents
                vl.ListItems.Clear();
                vl.ListItems.Add(new GH_ValueListItem("Dowel", "\"Dowel\""));
                vl.ListItems.Add(new GH_ValueListItem("Bolt", "\"Bolt\""));
                vl.ListItems.Add(new GH_ValueListItem("Screw", "\"Screw\""));
                vl.ListItems.Add(new GH_ValueListItem("Nail", "\"Nail\""));
                document.AddObject(vl, false);
                Params.Input[0].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[0].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 120, currPivot.Y - 11);
            }
            base.AddedToDocument(document);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Fastener Type", "Ftype", "Fastener type", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fu", "Fu", "Fastener ultimate strength [kN/cm²]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Fastener Length", "L", "Fastener length [mm]", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Fastener Diameter", "D", "Fastener Nominal Diameter [mm]", GH_ParamAccess.item, 0);
            Params.Input[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Fastener(), "Fastener", "Fast.", "Fastener properties", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double L = 0;
            double D = 0;
            double Ds = 0;
            double Dh = 0;
            double Fu = 0;
            bool Smooth = true;
            Fastener fastener = new Fastener();

            DA.GetData(0, ref Ftype);
            DA.GetData(1, ref Fu);
            DA.GetData(2, ref L);
            DA.GetData(3, ref D);

            if (Ftype == "Dowel" || Params.Input.Count == 4)
            {

            }
            else if (Ftype == "Bolt")
            {
                DA.GetData(4, ref Dh);
            }
            else if (Ftype == "Screw")
            {
                DA.GetData(4, ref Dh);
                DA.GetData(5, ref Ds);
            }
            else if (Ftype == "Nail")
            {
                DA.GetData(4, ref Dh);
                DA.GetData(5, ref Ds);
                DA.GetData(6, ref Smooth);
            }

            fastener = new Fastener(Ftype, D, Ds, Dh, L, Fu, Smooth);
            DA.SetData(0, new GH_Fastener(fastener));
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FDC62C6D-7C03-412D-8FF8-B76439197730"); }
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
            if (Ftype == "Screw")
            {

                if (index == 4)
                {
                    name = "Head Diameter";
                    nickname = "Dh";
                    description = "Fastener head diameter [mm]";
                }
                else if (index == 5)
                {
                    name = "Shank Diameter";
                    nickname = "Ds";
                    description = "Fastener shank diameter [mm]";
                }

            }
            else if (Ftype == "Bolt")
            {
                if (index == 4)
                {
                    name = "Head Diameter";
                    nickname = "Dh";
                    description = "Fastener head diameter [mm]";
                }
            }
            else if (Ftype == "Dowel")
            {
            }
            else if (Ftype == "Nail")
            {

                if (index == 4)
                {
                    name = "Head Diameter";
                    nickname = "Dh";
                    description = "Fastener head diameter [mm]";
                }
                else if (index == 5)
                {
                    name = "Shank Diameter";
                    nickname = "Ds";
                    description = "Fastener shank diameter [mm]";
                }
                else if (index == 6)
                {
                    name = "Smooth Shank";
                    nickname = "Smooth";
                    description = "True for smooth nail shank, false for other.";
                }
            }
            param.Name = name;
            param.NickName = nickname;
            param.Description = description;
            param.Optional = true;
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            //This function will be called when a parameter is about to be removed. 
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

            if (Ftype == "Screw")
            {
                if (Params.Input.Count == 6) return;
                else
                {
                    int param_remains = Params.Input.Count - 4;
                    for (int i = param_remains - 1; i >= 0; i--)
                    {
                        Params.Input[i + 4].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[i + 4]);
                    }
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 4));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 5));
                }
            }

            else if (Ftype == "Bolt")
            {
                if (Params.Input.Count == 5) return;
                else
                {
                    int param_remains = Params.Input.Count - 4;
                    for (int i = param_remains - 1; i >= 0; i--)
                    {
                        Params.Input[i + 4].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[i + 4]);
                    }
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 4));
                }
            }

            else if (Ftype == "Dowel")
            {
                if (Params.Input.Count == 4) return;
                else
                {
                    int param_remains = Params.Input.Count - 4;
                    for (int i = param_remains - 1; i >= 0; i--)
                    {
                        Params.Input[i + 4].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[i + 4]);
                    }
                }
            }

            else if (Ftype == "Nail")
            {
                if (Params.Input.Count == 7) return;
                else
                {
                    int param_remains = Params.Input.Count - 4;
                    for (int i = param_remains - 1; i >= 0; i--)
                    {
                        Params.Input[i + 4].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[i + 4]);
                    }
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 4));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 5));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 6));
                }
                #endregion

            }
        }
    }
}