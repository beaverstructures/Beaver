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

        string Ftype = "Screw";

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
                vl.ListItems.Add(new GH_ValueListItem("Screw", "\"Screw\""));
                vl.ListItems.Add(new GH_ValueListItem("Bolt", "\"Bolt\""));
                vl.ListItems.Add(new GH_ValueListItem("Dowel", "\"Dowel\""));
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
            pManager.AddTextParameter("Fastener Type", "Ftype", "Fastener Type", GH_ParamAccess.item);
            Params.Input[1].Optional = true;
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

            Fastener fastener = new Fastener();
            DA.GetData(0, ref Ftype);

            double L = 0;
            double D = 0;
            double Ds = 0;
            double Dh = 0;
            double Fu = 0;
            bool Smooth = true;

            if (Ftype == "Screw")
            {
                DA.GetData(1, ref Fu);
                DA.GetData(2, ref L);
                DA.GetData(3, ref D);
                DA.GetData(4, ref Ds);
                DA.GetData(5, ref Dh);
            }
            else if (Ftype == "Bolt")
            {
                DA.GetData(1, ref Fu);
                DA.GetData(2, ref L);
                DA.GetData(3, ref D);
                DA.GetData(4, ref Dh);
            }
            else if (Ftype == "Dowel")
            {
                DA.GetData(1, ref Fu);
                DA.GetData(2, ref L);
                DA.GetData(3, ref D);
            }
            else if (Ftype == "Nail")
            {
                DA.GetData(1, ref Fu);
                DA.GetData(2, ref L);
                DA.GetData(3, ref D);
                DA.GetData(4, ref Ds);
                DA.GetData(5, ref Dh);
                DA.GetData(6, ref Smooth);
            }
            else
            {
                throw new ArgumentException("Something wrong with Ftype");
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
                if (index == 1)
                {
                    name = "Fastener Fu";
                    nickname = "Fu";
                    description = "Fastener tensile ultimate strength [kN/cm²]";
                }
                else if (index == 2)
                {
                    name = "Length";
                    nickname = "L";
                    description = "Fastener length [mm]";
                }
                else if (index == 3)
                {
                    name = "Diameter";
                    nickname = "D";
                    description = "Fastener nominal diameter [mm]";
                }
                else if (index == 4)
                {
                    name = "Shank Diameter";
                    nickname = "Ds";
                    description = "Fastener shank diameter [mm]";
                }
                else if (index == 5)
                {
                    name = "Head Diameter";
                    nickname = "Dh";
                    description = "Fastener head diameter [mm]";
                }

            }
            else if (Ftype == "Bolt")
            {
                if (index == 1)
                {
                    name = "Fastener Fu";
                    nickname = "Fu";
                    description = "Fastener tensile ultimate strength [kN/cm²]";
                }
                else if (index == 2)
                {
                    name = "Length";
                    nickname = "L";
                    description = "Fastener length [mm]";
                }
                else if (index == 3)
                {
                    name = "Diameter";
                    nickname = "D";
                    description = "Fastener nominal diameter [mm]";
                }
                else if (index == 4)
                {
                    name = "Head Diameter";
                    nickname = "Dh";
                    description = "Fastener head diameter [mm]";
                }
            }
            else if (Ftype == "Dowel")
            {
                if (index == 1)
                {
                    name = "Fastener Fu";
                    nickname = "Du";
                    description = "Fastener tensile ultimate strength [kN/cm²]";
                }
                else if (index == 2)
                {
                    name = "Length";
                    nickname = "L";
                    description = "Fastener length [mm]";
                }
                else if (index == 3)
                {
                    name = "Diameter";
                    nickname = "D";
                    description = "Fastener nominal diameter [mm]";
                }

            }
            else if (Ftype == "Nail")
            {
                if (index == 1)
                {
                    name = "Fastener Fu";
                    nickname = "Fu";
                    description = "Fastener tensile ultimate strength [kN/cm²]";
                }
                else if (index == 2)
                {
                    name = "Length";
                    nickname = "L";
                    description = "Fastener length [mm]";
                }
                else if (index == 3)
                {
                    name = "Diameter";
                    nickname = "D";
                    description = "Fastener nominal diameter [mm]";
                }
                else if (index == 4)
                {
                    name = "Shank Diameter";
                    nickname = "Ds";
                    description = "Fastener shank diameter [mm]";
                }
                else if (index == 5)
                {
                    name = "Head Diameter";
                    nickname = "Dh";
                    description = "Fastener head diameter [mm]";
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
                if (Params.Input.Count == 5) return;
                else
                {
                    if (Params.Input.Count > 1)
                    {
                        Params.Input[1].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[1]);
                    }
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 1));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 2));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 3));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 4));
                }
            }

            else if (Ftype == "Bolt")
            {
                if (Params.Input.Count == 4) return;
                else
                {
                    if (Params.Input.Count > 1)
                    {
                        Params.Input[1].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[1]);
                    }
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 1));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 2));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 3));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 4));
                }
            }

            else if (Ftype == "Dowel")
            {
                if (Params.Input.Count == 3) return;
                else
                {
                    if (Params.Input.Count > 1)
                    {
                        Params.Input[1].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[1]);
                    }
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 1));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 2));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 3));
                }
            }

            else if (Ftype == "Nail")
            {
                if (Params.Input.Count == 6) return;
                else
                {
                    if (Params.Input.Count > 1)
                    {
                        Params.Input[1].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[1]);
                    }
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 1));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 2));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 3));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 4));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 5));
                    Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 6));
                }
                #endregion


            }
        }
    }