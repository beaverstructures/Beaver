using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using BeaverCore.Materials;
using BeaverCore.Connections;

namespace BeaverGrasshopper.Components.ConnectionComponents
{
    public class Comp_T2TConnection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Comp_T2TConnection()
          : base("Timber to Timber Connection", "T2T",
              "Assembles a timber to timber connection",
              "Beaver", "2. Connection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Fastener(), "Fastener", "Fast", "Beaver fastener element", GH_ParamAccess.item);
            pManager.AddBooleanParameter("preDrilled1", "preDrilled1", "Boolean representing whether timber element 1 must be predrilled", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("preDrilled2", "preDrilled2", "Boolean representing whether timber element 2 must be predrilled", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("alpha1", "alpha1", "alpha1 in degrees", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("alpha2", "alpha2", "alpha2 in degrees", GH_ParamAccess.item, 90);
            pManager.AddNumberParameter("t1", "t1", "t1 [mm]", GH_ParamAccess.item, 15);
            pManager.AddNumberParameter("t2", "t2", "t2 [mm]", GH_ParamAccess.item, 15);
            pManager.AddParameter(new Param_Material(), "mat1", "mat1", "mat1", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Material(), "mat2", "mat2", "mat2", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Shear Planes", "n", "Number of shear planes on the fastener. Currently beaver only supports 1 and 2 shear planes.", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("RopeEffect", "Rope?", "Boolean indicating whether tthe rope effect should be considered", GH_ParamAccess.item, false);
            pManager[8].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_T2TCapacity(), "Fastener capacity", "T2T", "Fastener capacity for a T2T check", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Fastener(), "Fastener", "Fast", "Beaver fastener with calculated capacity", GH_ParamAccess.item);
            pManager.AddTextParameter("Shear Capacities", "ShearCap", "Calculated capacities and respective failure modes",GH_ParamAccess.list);
            pManager.AddTextParameter("Axial Capacities", "AxialCap", "Calculated capacities and respective failure modes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Fastener ghfastener = new GH_Fastener();
            bool preDrilled1 = false;
            bool preDrilled2 = false;
            double alpha1 = 0;
            double alpha2 = 90;
            double t1 = 0;
            double t2 = 0;
            int shearplanes = 0;
            bool rope = false;
            GH_Material ghmat1 = new GH_Material();
            GH_Material ghmat2 = new GH_Material();

            DA.GetData(0, ref ghfastener);
            DA.GetData(1, ref preDrilled1);
            DA.GetData(2, ref preDrilled2);
            DA.GetData(3, ref alpha1);
            DA.GetData(4, ref alpha2);
            DA.GetData(5, ref t1);
            DA.GetData(6, ref t2);
            DA.GetData(7, ref ghmat1);
            DA.GetData(8, ref ghmat2);
            DA.GetData(9, ref shearplanes);
            DA.GetData(10, ref rope);

            // unit conversions to SI
            alpha1 = alpha1 * (Math.PI / 180);
            alpha2 = alpha2 * (Math.PI / 180);
            t1 = t1;
            t2 = t2;

            T2TCapacity t2TCapacity = new T2TCapacity(ghfastener.Value,
                                                      preDrilled1,
                                                      preDrilled2,
                                                      ghmat1.Value,
                                                      ghmat2.Value,
                                                      alpha1,
                                                      alpha2,
                                                      0,  // *** DOUBLE CHECK THIS INPUT LATER
                                                      t1,
                                                      t2,
                                                      shearplanes,
                                                      rope);

            List<string> axials = new List<string>();
            List<string> shears = new List<string>();

            foreach (KeyValuePair<string, double> keyValuePair in t2TCapacity.axial_capacities)
            {
                axials.Add(keyValuePair.Key + ": " + Math.Round(keyValuePair.Value,2));
            }
            foreach (KeyValuePair<string, double> keyValuePair in t2TCapacity.shear_capacities)
            {
                shears.Add(keyValuePair.Key + ": " + Math.Round(keyValuePair.Value, 2));
            }

            DA.SetData(0,new GH_T2TCapacity(t2TCapacity));
            DA.SetData(1, new GH_Fastener(t2TCapacity.fastener));
            DA.SetDataList(2, shears);
            DA.SetDataList(3, axials);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.TimberToTimberConnection;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2fc82eb2-7248-444f-98a2-51f77eef0df0"); }
        }
    }
}