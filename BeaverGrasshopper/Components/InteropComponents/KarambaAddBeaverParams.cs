using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using Karamba.GHopper;
using Karamba.Elements;
using Karamba.Results;
using Karamba.Geometry;
using Karamba.CrossSections;

using BeaverCore.Materials;

namespace BeaverGrasshopper.Components.InteropComponents
{
    public class KarambaAddBeaverParams : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public KarambaAddBeaverParams()
          : base("Karamba add beaver parameters", "AddBeaverParams",
              "Adds Beaver parameters to a Karamba Beam Element for performing ULS and SLS checks in Beaver",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Karamba.GHopper.Elements.Param_Element(), "Beam", "Beam","Karamba Beam element to be Modified", GH_ParamAccess.item);
            pManager.AddNumberParameter("Span Length", "SpanL", "Span distance for SLS check of beam", GH_ParamAccess.item);
            pManager.AddNumberParameter("Buckling Length Y" , "BklLenY", "Buckling length of element in local Y-direction if > 0.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Buckling Length Z" , "BklLenZ", "Buckling length of element in local Z - direction if > 0.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Buckling Length LT", "BklLenLT", "Buckling length of element for lateral torsional buckling if > 0.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Cantilever", "Cantilever", "Boolean parameter describing whether the beam is cantilevered. By default is set to False", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Karamba.GHopper.Elements.Param_Element(), "Beam", "Beam", "Karamba Beam element to be Modified", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Karamba.GHopper.Elements.GH_Element> beams = new List<Karamba.GHopper.Elements.GH_Element>();
            List<Karamba.GHopper.Elements.GH_Element> out_beams = new List<Karamba.GHopper.Elements.GH_Element>();

            List<double> spans = new List<double>();
            List<bool> cantilevers = new List<bool>();
            List<double> bklY = new List<double>();
            List<double> bklZ = new List<double>();
            List<double> bklLT = new List<double>();
            
            DA.GetDataList(0, beams);
            DA.GetDataList(1, spans);
            DA.GetDataList(2, bklY);
            DA.GetDataList(3, bklZ);
            DA.GetDataList(4, bklLT);
            DA.GetDataList(5, cantilevers);

            for (int i = 0; i < beams.Count; i++)
            {
                BuilderElementStraightLine beam = beams[i].Value as BuilderElementStraightLine;
                beam.UserData.Add("SpanLength", spans[i]);
                beam.BucklingLength_Set(BuilderElementStraightLine.BucklingDir.bklY, bklY[i]);
                beam.BucklingLength_Set(BuilderElementStraightLine.BucklingDir.bklZ, bklZ[i]);
                beam.BucklingLength_Set(BuilderElementStraightLine.BucklingDir.bklLT, bklLT[i]);
                beam.UserData.Add("Cantilever", spans[i]);

                out_beams.Add(new Karamba.GHopper.Elements.GH_Element(beam));
            }


            DA.SetDataList(0, out_beams);

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
            get { return new Guid("fe2a3ffb-9774-43b9-8785-78683c8617ce"); }
        }
    }
}