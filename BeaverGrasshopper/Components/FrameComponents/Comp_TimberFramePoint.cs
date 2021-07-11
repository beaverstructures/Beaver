using BeaverCore.Actions;
using BeaverCore.CrossSection;
using BeaverCore.Frame;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BeaverGrasshopper
{
    public class TimberFramePointComp : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TimberFramePoint class.
        /// </summary>
        public TimberFramePointComp()
          : base("TimberFramePoint", "FramePoint",
              "Definition of a single timber frame point",
              "Beaver", "1.Frame")
        {
        }

        public override void AddedToDocument(GH_Document document)
        {
            // Perform Layout to get actual positionning of the component on the canevas
            this.Attributes.ExpireLayout();
            this.Attributes.PerformLayout();

            //instantiate new value list
            var cro_sec = new Comp_CrossSection();
            var material = new Comp_Materiall();
            var force = new Comp_Force();
            var displacement = new Comp_Displacement();

            document.AddObject(cro_sec, false);
            Params.Input[0].AddSource(cro_sec.Params.Output[0]);
            PointF currPivot = Params.Input[0].Attributes.Pivot;
            cro_sec.Attributes.Pivot = new PointF(currPivot.X - 300, currPivot.Y-50);
            cro_sec.Params.Input[0].Sources[0].Attributes.Pivot = new PointF(currPivot.X - 450, currPivot.Y-91);
            document.AddObject(material, false);
            cro_sec.Params.Input[1].AddSource(material.Params.Output[0]);
            material.Attributes.Pivot = new PointF(currPivot.X - 600, currPivot.Y - 61);

            document.AddObject(force, false);
            Params.Input[1].AddSource(force.Params.Output[0]);
            force.Attributes.Pivot = new PointF(currPivot.X - 300, currPivot.Y +90);

            document.AddObject(displacement, false);
            Params.Input[2].AddSource(displacement.Params.Output[0]);
            displacement.Attributes.Pivot = new PointF(currPivot.X - 300, currPivot.Y +220);

            base.AddedToDocument(document);
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CroSec(), "Cross Section", "CroSec", "Frame Cross Section", GH_ParamAccess.item);
        pManager.AddParameter(new Param_Force(), "Forces", "Forces", "Frame nodal forces", GH_ParamAccess.list);
        pManager.AddParameter(new Param_Displacement(), "Displacement Vector", "Disp.", "Vector of point displcaement", GH_ParamAccess.list);
        pManager.AddNumberParameter("Buckling Lenght Y", "bL_y", "Normal force at point", GH_ParamAccess.item, 3);
        pManager.AddNumberParameter("Buckling Lenght Z", "bL_s", "Normal force at point", GH_ParamAccess.item, 3);
        pManager.AddNumberParameter("Span lenght", "sL", "Normal force at point", GH_ParamAccess.item, 3);
        pManager.AddNumberParameter("Pre camber", "Pcamber", "Pre camber", GH_ParamAccess.item, 0);
        pManager.AddTextParameter("Span Type", "Stype", "Simple span or cantilever", GH_ParamAccess.item, "SimpleSpan");
        pManager.AddNumberParameter("Span Limit Range", "Srange",
            "Parameter in domain [0-1] to set limits between range defined in EC5 Table 7.2", GH_ParamAccess.item, 0.5);
        pManager.AddIntegerParameter("Service Class", "SC", "Service Class according to EC5", GH_ParamAccess.item, 0);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_TFPoint(), "TimberFramePoint", "TFPoint", "Timber Frame Point", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CroSec ghcrosec = new GH_CroSec();
        List<GH_Force> ghforce = new List<GH_Force>();
        List<GH_Displacement> ghdisplacement = new List<GH_Displacement>();
        double bl_y = 0;
        double bl_z = 0;
        double sl = 0;
        double pre_camber = 0;
        string span_Type = "";
        double span_range = 0;
        int service_class = 0;
        DA.GetData(0, ref ghcrosec);
        DA.GetDataList(1, ghforce);
        DA.GetDataList(2, ghdisplacement);
        DA.GetData(3, ref bl_y);
        DA.GetData(4, ref bl_z);
        DA.GetData(5, ref sl);
        DA.GetData(6, ref pre_camber);
        DA.GetData(7, ref span_Type);
        DA.GetData(8, ref span_range);
        DA.GetData(9, ref service_class);
        CroSec crosec = ghcrosec.Value;
        List<Force> forces = new List<Force>();
        List<Displacement> displacements = new List<Displacement>();
        for (int i = 0; i < forces.Count; i++)
        {
            Force force = ghforce[i].Value;
            forces.Add(force);
            Displacement disp = ghdisplacement[i].Value;
            displacements.Add(disp);
        }
        //Think of ways of simplifying this input, it still needs span type and span limits
        TimberFramePoint timber_frame_point = new TimberFramePoint(forces, displacements, crosec, service_class, bl_y, bl_z, sl, 0.9);

        DA.SetData(0, new GH_TimberFramePoint(timber_frame_point));
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
            return Properties.Resources.TimberFrame;
        }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
        get { return new Guid("b6fb1405-42ff-4131-a1dd-b55b170cd60d"); }
    }
}
}