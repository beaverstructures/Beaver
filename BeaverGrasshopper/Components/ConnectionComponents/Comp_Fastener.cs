using BeaverCore.Actions;
using BeaverCore.Connections;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;

using Rhino.Geometry;

using System;
using System.Collections.Generic;
using System.Drawing;


namespace BeaverGrasshopper
{
    public class Comp_Fastener : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Comp_Force class.
        /// </summary>
        public Comp_Fastener()
          : base("Fastener", "Fast",
              "Creates Beaver fastener element for assembly into a Beaver connection element",
              "Beaver", "2. Connection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("FastenerType", "Ftype", "Fastener type", GH_ParamAccess.item);
            pManager.AddNumberParameter("Nominal Diameter", "D", "Fastener nominal diamater [mm]", GH_ParamAccess.item, 6);
            pManager.AddNumberParameter("Shank Diameter", "Ds", "Fastener shank diameter [mm]", GH_ParamAccess.item, 6); //screws
            pManager.AddNumberParameter("Head Diameter", "Dh", "Fastener head diameter [mm]", GH_ParamAccess.item, 6);
            pManager.AddNumberParameter("Fastener Length", "L", "Fastener length [mm]", GH_ParamAccess.item, 50);
            pManager.AddBooleanParameter("Smooth Boolean", "Smooth", "True for smooth nails, false for other", GH_ParamAccess.item, false); //nails
            pManager.AddNumberParameter("Fastener Fuk", "Fuk", "Fastener steel characteristic tensile ultimate strength [MPa]", GH_ParamAccess.item, 400);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Fastener(), "Fastener", "Fast", "Beaver fastener element", GH_ParamAccess.item);
        }


        string Ftype = "Ftype?";
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            double D = 0;
            double Ds = 0;
            double Dh = 0;
            double L = 0;
            bool Smooth = true;
            double Fuk = 0;

            DA.GetData(0, ref Ftype);
            DA.GetData(1, ref D);
            DA.GetData(2, ref Ds);
            DA.GetData(3, ref Dh);
            DA.GetData(4, ref L);
            DA.GetData(5, ref Smooth);
            DA.GetData(6, ref Fuk);

            Fastener fastener = new Fastener(Ftype, D, Ds, Dh, L, Fuk, Smooth);
            DA.SetData(0, new GH_Fastener(fastener));
        }

        #region GH CANVAS
        private enum fastenerTypes
        {
            Dowel,
            Screw,
            Bolt,
            Nail
        }

        public override void AddedToDocument(GH_Document document)
        {
            if (Params.Input[1].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate utilization type list
                var vl = new GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "Type";
                //clear default contents
                vl.ListItems.Clear();
                foreach (fastenerTypes util_type in Enum.GetValues(typeof(fastenerTypes)))
                    vl.ListItems.Add(new GH_ValueListItem(util_type.ToString(), "\"" + util_type.ToString() + "\""));
                document.AddObject(vl, false);
                Params.Input[1].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[1].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 250, currPivot.Y - 11);
            }
            base.AddedToDocument(document);
        }

        protected override void AfterSolveInstance()
        {
            VariableParameterMaintenance();
            Params.OnParametersChanged();
        }

        #region VARIABLE COMPONENT INTERFACE IMPLEMENTATION
        public bool CanInsertParameter(GH_ParameterSide side, int index) { return false; }
        public bool CanRemoveParameter(GH_ParameterSide side, int index) { return false; }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var param = new Param_Number
            {   
                Name = "C",
                NickName = "C",
                Description = "C",
                Access = GH_ParamAccess.item,
                Optional = true
            };
            param.AddVolatileData(new Grasshopper.Kernel.Data.GH_Path(0), index, 100);
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
            switch (Ftype)
            {
                case "Dowel":
                    break;
                case "Screw":
                case "Bolt":
                case"Nail":
                    //if (Params.Input.Count != 3) return;
                    //Params.Input[2].Sources.Clear();
                    //Params.UnregisterInputParameter(Params.Input[2]);
                    break;
            }
        }


        #endregion
        #endregion


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Bolt;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ce157802-fa6c-44f1-846b-b4c6092649cf"); }
        }
    }
}