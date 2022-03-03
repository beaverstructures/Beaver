using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Karamba.Models;
using Karamba.GHopper.Models;

using BeaverCore.Frame;
using BeaverCore.Actions;
using BeaverCore.Materials;
using BvGeom = BeaverCore.Geometry;

using Karamba.Elements;
using Karamba.Results;
using Karamba.Geometry;
using Karamba.CrossSections;
using static Karamba.Elements.BuilderElementStraightLine;

using BeaverGrasshopper.Components.InteropComponents;


namespace BeaverGrasshopper
{
    public class KarambaToBeaver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the KarambaToBeaver class.
        /// </summary>
        public KarambaToBeaver()
          : base("KarambaToBeaver", "K2B",
              "Retrieves TimberFrames from Karamba beams",
              "Beaver", "External")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Karamba Model", "Model",
                        "Karamba model with calculated displacements", GH_ParamAccess.item);
            pManager.AddTextParameter("Beam Identifiers", "BeamsIds", "Beam ID", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number of results per beam element", "NRes", "Element nodal subdivision", GH_ParamAccess.item,2);
            pManager.AddTextParameter("LoadCase Type", "LCType", "Load Case Type", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Run", "Run", "Boolean for running the algorithm", GH_ParamAccess.item,false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_TimberFrame(), "TimberFrames", "TF's", "Timber Frames from Karamba beams", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Model gh_model = new GH_Model();
            List<string> beam_id = new List<string>();
            List<string> lc_types = new List<string>();
            int sub_div = 0;
            GH_Material gh_material = new GH_Material();
            bool run = false;

            DA.GetData(0, ref gh_model);
            DA.GetDataList(1, beam_id);
            DA.GetData(2, ref sub_div);
            DA.GetDataList(3, lc_types);
            DA.GetData(4, ref run);

            List<GH_TimberFrame> timber_frames = null;
            if (run)
            {
                Model model = gh_model.Value;
                List<List<List<List<double>>>> force_results = new List<List<List<List<double>>>>();
                List<List<List<Vector3>>> trans_displacement_results = new List<List<List<Vector3>>>();
                List<List<List<Vector3>>> rot_displacement_results = new List<List<List<Vector3>>>();
                Karamba.Results.BeamForces.solve(
                    model, beam_id, null, 100000, sub_div + 1, out force_results);
                Karamba.Results.BeamDisplacements.solve(
                    model, beam_id, null, 100000, sub_div + 1, out trans_displacement_results, out rot_displacement_results);
                List<ModelElement> beams = model.elementsByID(beam_id);
                List<Force>[,] elements_forces = new List<Force>[beams.Count, sub_div + 1];
                List<Displacement>[,] elements_displacements = new List<Displacement>[beams.Count, sub_div + 1];

                for (int i = 0; i < force_results.Count; i++)
                {
                    if (lc_types.Count - 1 >= i)
                    {
                        Parallel.For(0, force_results[i].Count, j =>
                        {
                            for (int k = 0; k < force_results[i][j].Count; k++)
                            {
                                if (elements_forces[j, k] is null) elements_forces[j, k] = new List<Force>();
                                if (elements_displacements[j, k] is null) elements_displacements[j, k] = new List<Displacement>();
                                Force force = new Force(force_results[i][j][k], lc_types[i], 1000);
                                Vector3 displacement_vec = trans_displacement_results[i][j][k];
                                Displacement displacement = new Displacement(displacement_vec.X,
                                                                             displacement_vec.Y,
                                                                             displacement_vec.Z,
                                                                             lc_types[i]);
                                elements_forces[j, k].Add(force);
                                elements_displacements[j, k].Add(displacement);
                            }
                        });
                    }
                    else
                    {
                        throw new ArgumentException("LCtype was not found.");
                    }

                }
                timber_frames = ExtendedMethods.CreateList<GH_TimberFrame>(beams.Count);
                Parallel.For(0, beams.Count, new ParallelOptions
                {
                    // multiply the count because a processor has 2 cores
                    MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0))
                }, i =>
                {
                    Dictionary<double, TimberFramePoint> TFPoints = new Dictionary<double, TimberFramePoint>();
                    ModelBeam modelBeam = beams[i] as ModelBeam;
                    BuilderElement beam = modelBeam.BuilderElement();
                    Material material = beam.crosec.material.K3DToBeaver();

                    double spanLength = modelBeam.elementLength(model);
                    bool local = false;
                    try
                    {
                        spanLength = (double)beam.UserData["SpanLength"];
                        local = true;
                    }
                    catch
                    {
                        if (!beam.UserData.ContainsKey("SpanLength")) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                                              "Beam does not contain span length data. Element length will be used");
                        else if (!(beam.UserData["SpanLength"] is double)) throw new Exception("SpanLength values must be double");
                    }
                    int serviceClass = 2;
                    try
                    {
                        serviceClass = (int)beam.UserData["ServiceClass"];
                    }
                    catch
                    {
                        serviceClass = 2;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                                              "Beam does not contain service class data. Service class 2 will be considered");
                    }
                    CroSec crosec = beam.crosec;
                    BeaverCore.CrossSection.CroSec beaver_crosec = beam.crosec.K3DToBeaver();
                    double rel_pos_step = modelBeam.elementLength(model) / (sub_div);
                    BvGeom.Point3D node1 = model.nodes[modelBeam.node_inds[0]].pos.K3Dpt2Beaver();
                    BvGeom.Point3D node2 = model.nodes[modelBeam.node_inds[1]].pos.K3Dpt2Beaver();
                    BvGeom.Line beaver_line = new BvGeom.Line(node1, node2);

                    // creates a SpanLine object with correct properties assigned from Karamba
                    Polyline poly = (Polyline)beam.UserData["SpanLine"];
                    List<BvGeom.Point3D> pts = new List<BvGeom.Point3D>();
                    foreach(Point3d pt in poly.ToList())
                    {
                        pts.Add(new BvGeom.Point3D(pt.X, pt.Y, pt.Z));
                    }
                    BvGeom.Polyline beaver_Polyline = new BvGeom.Polyline(pts);
                    BeaverCore.Frame.TimberFrame.SpanLine spanLine = ExtendedMethods.ImportSpanLineProperties(
                        beaver_Polyline,
                        model,
                        lc_types);

                    for (int j = 0; j < sub_div + 1; j++)
                    {
                        TimberFramePoint TFPoint = new TimberFramePoint(
                            beaver_line.PointAtRelativePosition(Convert.ToDouble(j) / sub_div),
                            elements_forces[i, j],
                            elements_displacements[i, j],
                            beaver_crosec,
                            (int)beam.UserData["ServiceClass"],
                            modelBeam.buckling_length(BucklingDir.bklY),
                            modelBeam.buckling_length(BucklingDir.bklZ),
                            spanLength, 0.9,
                            (bool)beam.UserData["Cantilever"],
                            local: (bool)beam.UserData["Local"],
                            _localRefDisps: spanLine.midDisp);
                        TFPoints[j * rel_pos_step] = TFPoint;
                    }
                    TimberFrame timber_frame = new TimberFrame(TFPoints, beaver_line, spanLine);
                    timber_frames[i] = new GH_TimberFrame(timber_frame);
                });
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                                              "Set Run to True for running the algorithm.");
            }

            DA.SetDataList(0, timber_frames);
        }
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.KarambaToTimberFrames;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4d798265-19cd-4524-8b9a-2f36f6994d81"); }
        }
    }
}