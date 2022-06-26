using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

using Karamba.GHopper;
using Karamba.Elements;
using Karamba.Results;
using Karamba.Geometry;
using Karamba.CrossSections;

using BeaverCore.Materials;
using BeaverGrasshopper.Components.Utilities;

namespace BeaverGrasshopper.Components.InteropComponents
{
    public class Comp_KarambaAddBeaverParams : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Comp_KarambaAddBeaverParams()
          : base("Karamba add beaver parameters", "B2KBeams",
              "Adds Beaver parameters to a Karamba Beam Element for performing ULS and SLS checks in Beaver",
              "Beaver", "External")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Karamba.GHopper.Elements.Param_Element(), "Beam", "Beam","Karamba Beam element to be Modified", GH_ParamAccess.list);
            pManager.AddCurveParameter("Span Line", "SpanLine", 
                "Span polyline or polyline representing the full undeformed length of the beam. This is used for defining local considerations of SLS. " +
                "If no line is provided, SLS will consider a global displacement", GH_ParamAccess.list);
            pManager.AddNumberParameter("Buckling Length Y" , "BklLenY", "Buckling length of element in local Y-direction if > 0.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Buckling Length Z" , "BklLenZ", "Buckling length of element in local Z - direction if > 0.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Buckling Length LT", "BklLenLT", "Buckling length of element for lateral torsional buckling if > 0.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Cantilever", "Cantilever", "Boolean parameter describing whether the beam is cantilevered. By default is set to False", GH_ParamAccess.list, false);
            pManager.AddIntegerParameter("Service Class", "SC", "Service Class from 1 to 3 regarding the timber element. By default is set to 2", GH_ParamAccess.list,2);
            pManager.AddNumberParameter("Precamber", "Pcamber", "Precamber of beam elements in meters. Default is set to 0", GH_ParamAccess.list, 0);
            pManager.AddBooleanParameter("Local", "Local", "Boolean parameter describing whether the beam should be calculated using global or local SLS analysis. By default is set to False", GH_ParamAccess.list, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
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

            List<Curve> spans = new List<Curve>();
            List<bool> cantilevers = new List<bool>();
            List<double> bklY = new List<double>();
            List<double> bklZ = new List<double>();
            List<double> bklLT = new List<double>();
            List<int> serviceClasses = new List<int>();
            List<double> precambers = new List<double>();
            List<bool> local = new List<bool>();

            DA.GetDataList(0, beams);
            DA.GetDataList(1, spans);
            DA.GetDataList(2, bklY);
            DA.GetDataList(3, bklZ);
            DA.GetDataList(4, bklLT);
            DA.GetDataList(5, cantilevers);
            DA.GetDataList(6, serviceClasses);
            DA.GetDataList(7, precambers);
            DA.GetDataList(8, local);

            List<Polyline> polylines = new List<Polyline>();
            if (spans.Count > 0)
            {
                foreach (Curve curve in spans)
                {
                    Polyline pl;
                    if (curve.TryGetPolyline(out pl))
                        polylines.Add(pl);
                    else throw new ArgumentException("Curve is not a polyline");
                }
            }
            

            // generates list of values if only one value is provided.
            cantilevers = (cantilevers.Count > 1) ? 
                cantilevers : Enumerable.Repeat(cantilevers[0], beams.Count).ToList();
            serviceClasses = (serviceClasses.Count > 1) ? 
                serviceClasses : Enumerable.Repeat(serviceClasses[0], beams.Count).ToList();
            precambers = (precambers.Count > 1) ?
                precambers : Enumerable.Repeat(precambers[0], beams.Count).ToList();
            local = (local.Count > 1) ?
                local : Enumerable.Repeat(local[0], beams.Count).ToList();

            for (int i = 0; i < beams.Count; i++)
            {
                BuilderElementStraightLine beam = beams[i].Value as BuilderElementStraightLine;
                if (!polylines[i].IsValid) {
                    polylines[i] = new Polyline();
                }

                List<BeaverCore.Geometry.Point3D> pts = new List<BeaverCore.Geometry.Point3D>();
                foreach (Point3d point in polylines[i].ToList())
                {
                    pts.Add(new BeaverCore.Geometry.Point3D(point.X, point.Y, point.Z));
                }
                
                BeaverCore.Geometry.Polyline beaverPoly = new BeaverCore.Geometry.Polyline(pts);

                beam.UserData["SpanLine"] = polylines[i];
                beam.UserData["SpanLength"] = polylines[i].First.DistanceTo(polylines[i].Last);
                beam.UserData["Cantilever"] = cantilevers[i];
                beam.UserData["ServiceClass"] = serviceClasses[i];
                beam.UserData["Precamber"] = precambers[i];
                beam.UserData["Local"] = local[i];
                beam.BucklingLength_Set(BuilderElementStraightLine.BucklingDir.bklY, bklY[i]);
                beam.BucklingLength_Set(BuilderElementStraightLine.BucklingDir.bklZ, bklZ[i]);
                beam.BucklingLength_Set(BuilderElementStraightLine.BucklingDir.bklLT, bklLT[i]);
                
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
                return Properties.Resources.Beaver_Param;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c8376373-268e-471f-ac61-933b148a1fd8"); }
        }
    }
}