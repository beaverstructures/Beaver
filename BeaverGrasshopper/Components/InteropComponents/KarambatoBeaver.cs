using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Karamba.Models;
using Karamba.GHopper.Models;

using BeaverCore.Frame;
using BeaverCore.Actions;
using Karamba.Elements;
using Karamba.Results;
using Karamba.Geometry;
using Karamba.CrossSections;
using BeaverCore.Materials;
using static Karamba.Elements.BuilderElementStraightLine;

namespace BeaverGrasshopper
{
    public class KarambatoBeaver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the KarambatoBeaver class.
        /// </summary>
        public KarambatoBeaver()
          : base("KarambatoBeaver", "Karamba",
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
            pManager.AddParameter(new Param_Material(), "Beaver Material", "Mat",
                        "Timber Material", GH_ParamAccess.item);
            pManager.AddTextParameter("Beam Identifiers", "BeamsIds", "Beam ID", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number of results per beam element", "NRes", "Element nodal subdivision", GH_ParamAccess.list);
            pManager.AddTextParameter("LoadCase Type", "LCType", "Load Case Type", GH_ParamAccess.list);
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
            DA.GetData(0, ref gh_model);
            DA.GetData(1, ref gh_material);
            DA.GetData(2, ref sub_div);
            DA.GetDataList(3, beam_id);
            DA.GetDataList(4, lc_types);
            Model model = gh_model.Value;
            Material material = gh_material.Value;
            List<List<List<List<double>>>> force_results = new List<List<List<List<double>>>>();
            List<List<List<Vector3>>> trans_displacement_results = new List<List<List<Vector3>>>();
            List<List<List<Vector3>>> rot_displacement_results = new List<List<List<Vector3>>>();
            BeamForces.solve(model, beam_id, -1, 100000, sub_div,out force_results);
            BeamDisplacements.solve(model, beam_id, -1, 100000, sub_div, out trans_displacement_results, out rot_displacement_results);
            List<List<List<Force>>> elements_forces = new List<List<List<Force>>>(force_results[0].Count);
            List<List<List<Displacement>>> elements_displacements = new List<List<List<Displacement>>>(force_results[0].Count);
            List<ModelElement> beams = model.elementsByID(beam_id);
            for (int i = 0; i < force_results.Count; i++)
            {
                for (int j = 0; j < force_results[i].Count; j++)
                {
                    for (int k = 0; k < force_results[i][j].Count; k++)
                    {
                        Force force = new Force(force_results[i][j][k], lc_types[i]);
                        Vector3 displacement_vec = trans_displacement_results[i][j][k];
                        Displacement displacement = new Displacement(displacement_vec.X, displacement_vec.Y, displacement_vec.Z, lc_types[i]);
                        elements_forces[j][k].Add(force);
                        elements_displacements[j][k].Add(displacement);
                    }
                }
            }
            List<TimberFrame> timber_frames = new List<TimberFrame>();
            for (int i = 0; i < elements_forces.Count; i++)
            {
                Dictionary<double, TimberFramePoint> TFPoints = new Dictionary<double, TimberFramePoint>();
                ModelBeam beam = (ModelBeam)beams[i];
                CroSec crosec = beam.crosec;
                BeaverCore.CrossSection.CroSec beaver_crosec = CroSecKarambatoBeaver(beam.crosec, material);
                double rel_pos_step = beam.elementLength(model) / elements_forces[i].Count;
                BeaverCore.Geometry.Point3D node1 = PointKarambatoBeaver(model.nodes[beam.node_inds[0]].pos);
                BeaverCore.Geometry.Point3D node2 = PointKarambatoBeaver(model.nodes[beam.node_inds[1]].pos);
                BeaverCore.Geometry.Line beaver_line = new BeaverCore.Geometry.Line(node1, node2);
                for (int j = 0; j < elements_forces[i].Count; j++)
                {
                    TimberFramePoint TFPoint = new TimberFramePoint(elements_forces[i][j], elements_displacements[i][j], beaver_crosec,
                        2, beam.buckling_length(BucklingDir.bklY), beam.buckling_length(BucklingDir.bklZ), beam.elementLength(model), 0.9);
                    TFPoints[j*rel_pos_step] = TFPoint;
                }
                TimberFrame timber_frame = new TimberFrame(TFPoints, beaver_line);
                timber_frames.Add(timber_frame);
            }
            DA.SetDataList(0, timber_frames);
        }

        BeaverCore.CrossSection.CroSec CroSecKarambatoBeaver(CroSec karamba_crosec, Material material)
        {
            if (karamba_crosec is CroSec_Trapezoid)
            {
                CroSec_Trapezoid trapezoid_crosec = (CroSec_Trapezoid)karamba_crosec;
                double width = Math.Min(trapezoid_crosec.lf_width, trapezoid_crosec.uf_width);
                BeaverCore.CrossSection.CroSec beaver_crosec = new BeaverCore.CrossSection.CroSec_Rect(trapezoid_crosec._height, width, material);
                return beaver_crosec;
            }
            else if (karamba_crosec is CroSec_Circle)
            {
                CroSec_Circle circle_crosec = (CroSec_Circle)karamba_crosec;
                BeaverCore.CrossSection.CroSec beaver_crosec = new BeaverCore.CrossSection.CroSec_Circ(circle_crosec.getHeight(),material);
                return beaver_crosec;
            }
            else
            {
                throw new ArgumentException("Karamba to Beaver Conversion only supports Karamba Trapezoid and Circle Cross Sections");
            }
        }

       BeaverCore.Geometry.Point3D PointKarambatoBeaver(Point3 karamba_point)
        {
            return new BeaverCore.Geometry.Point3D(karamba_point.X, karamba_point.Y, karamba_point.Z);
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