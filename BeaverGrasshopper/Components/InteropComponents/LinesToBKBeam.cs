using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Karamba.CrossSections;
using Karamba.Elements;
using Karamba.Geometry;
using Karamba.GHopper.CrossSections;
using Karamba.GHopper.Elements;
using Karamba.GHopper.Utilities;
using Karamba.GHopper.Utilities.UIWidgets;
using Karamba.Properties;
using Karamba.Utilities;
using Rhino.Geometry;
namespace BeaverGrasshopper
{
    /*
    public class AddSpanLengthToKaramba : GH_Component
    {
        
        private static readonly bool default_remove_dup = true;
        private static readonly bool default_bending = true;
        private static readonly bool default_set_bkl_len = true;
        private static readonly bool default_new = true;
        private static readonly double default_limit_dist = 0.005;
        private static readonly string default_id = string.Empty;
        private static readonly double default_ToPolyline_maxAngleRadians = 0.03;
        private static readonly double default_ToPolyline_Tolerance = 0.01;
        private static readonly double default_ToPolyline_minEdgeLength = 0.25;
        private static Vector3d default_z_axis = new Vector3d(0.0, 0.0, 1.0);
        
        /// <summary>
        /// Initializes a new instance of the AddSpanLengthToKaramba class.
        /// </summary>
        public AddSpanLengthToKaramba()
          : base("AddSpanLengthToKaramba", "Karamba",
              "Creates Karamba beam element from lines, crossections, span lengths and buckling lengths",
              "Beaver", "External")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            UnitsConversionFactory unitsConversionFactory = UnitsConversionFactory.Conv();
            UnitConversion unitConversion = unitsConversionFactory.cm();
            UnitConversion unitConversion2 = unitsConversionFactory.m();
            UnitConversion unitConversion3 = unitsConversionFactory.kN();

            pManager.AddCurveParameter("Line", "Line", "Line which will be connected to others if they meet at common points. Node indexes may change if lines are added.", (GH_ParamAccess)1);
            pManager.AddColourParameter("Color", "Color", "Color of the beam. The component applies the longest list principle with respect to the input lines.", (GH_ParamAccess)1);
            pManager.AddTextParameter("Identifier", "Id", "Identifier of the beam. Need not be unique in a model. The component applies the longest list principle with respect to the input lines.", (GH_ParamAccess)1, default_id);
            pManager.AddParameter(new Param_CrossSection(), "Cross section", "CroSec", "Cross section of the beam. The component applies the longest list principle with respect to the input lines.", (GH_ParamAccess)1);
            pManager.AddParameter(new Param_Boolean(), "Consider Buckling", "Buckling", "If true cross section optimization of the element is done considering buckling. Deactivating buckling can be useful for simulating slender elements which you want to pretension in reality but not in the numerical model.", (GH_ParamAccess)0);
            pManager.AddParameter(new Param_Number(), "Buckling Length Y" + unitConversion2.unitB, "BklLenY", "Buckling length " + unitConversion2.unitB + " of element in local Y-direction if > 0.", (GH_ParamAccess)0);
            pManager.AddParameter(new Param_Number(), "Buckling Length Z" + unitConversion2.unitB, "BklLenZ", "Buckling length " + unitConversion2.unitB + " of element in local Z-direction if > 0.", (GH_ParamAccess)0);
            pManager.AddParameter(new Param_Number(), "Buckling Length LT" + unitConversion2.unitB, "BklLenLT", "Buckling length " + unitConversion2.unitB + " of element for lateral torsional buckling if > 0.", (GH_ParamAccess)0);
            pManager.AddParameter(new Param_Number(), "Span length" + unitConversion2.unitB, "SpanLen", "Span length " + unitConversion2.unitB + " of element for Beaver SLS analysis", (GH_ParamAccess)0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam((IGH_Param)(object)new Param_Element(), "Element", "Elem", "Beams with default properties");
            pManager.Register_StringParam("Info", "Info", "Information regarding the conversion of lines to beams");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            UnitConversion base_length = UnitsConversionFactory.Conv().base_length;
		List<GH_Curve> list = new List<GH_Curve>();
		List<GH_Point> list2 = new List<GH_Point>();
		List<GH_Vector> list3 = new List<GH_Vector>();
		List<GH_String> list4 = new List<GH_String>();
		List<GH_Colour> list5 = new List<GH_Colour>();
		List<GH_CrossSection> list6 = new List<GH_CrossSection>();
		if (!DA.GetDataList<GH_Curve>(0, list) || list == null || list.Count == 0)
		{
			return;
		}
		if (DA.GetDataList<GH_Colour>(1, list5))
		{
			Utils.blowUp(list5, list.Count);
		}
		if (!DA.GetDataList<GH_String>(2, list4))
		{
			list4.Add(new GH_String(default_id));
		}
		Utils.blowUp(list4, list.Count);
		if (DA.GetDataList<GH_CrossSection>(3, list6))
		{
			Utils.blowUp(list6, list.Count);
		}
		DA.GetDataList<GH_Point>(4, list2);
		bool new_nodes = default_new;
		DA.GetData<bool>(5, ref new_nodes);
		bool remove_dup = default_remove_dup;
		DA.GetData<bool>(6, ref remove_dup);
		double num = INIReader.Instance().asDouble("limit_dist", default_limit_dist);
		if (DA.GetData<double>(7, ref num))
		{
			num = base_length.toBase(num);
		}
		if (DA.GetDataList<GH_Vector>(8, list3))
		{
			Utils.blowUp(list3, list.Count);
		}
		GH_Boolean val = new GH_Boolean(default_bending);
		DA.GetData<GH_Boolean>(9, ref val);
		bool value = ((GH_Goo<bool>)(object)val).get_Value();
		GH_Boolean val2 = new GH_Boolean(default_set_bkl_len);
		DA.GetData<GH_Boolean>(10, ref val2);
		bool value2 = ((GH_Goo<bool>)(object)val2).get_Value();
		double toPolyline_maxAngleRadians = INIReader.Instance().asDouble("ToPolyline_maxAngleRadians", default_ToPolyline_maxAngleRadians);
		DA.GetData<double>(11, ref toPolyline_maxAngleRadians);
		double toPolyline_Tolerance = INIReader.Instance().asDouble("ToPolyline_Tolerance", default_ToPolyline_Tolerance);
		DA.GetData<double>(12, ref toPolyline_Tolerance);
		double toPolyline_minEdgeLength = INIReader.Instance().asDouble("ToPolyline_minEdgeLength", default_ToPolyline_minEdgeLength);
		DA.GetData<double>(13, ref toPolyline_minEdgeLength);
		IEnumerable<IEnumerable<Line3>> in_lines = list.Select((GH_Curve gh_curve) => GHUtils.GetLineInput(((GH_Goo<Curve>)(object)gh_curve).get_Value(), toPolyline_maxAngleRadians, toPolyline_Tolerance, toPolyline_minEdgeLength));
		IEnumerable<double> bklLens = null;
		if (value2)
		{
			bklLens = list.Select(delegate(GH_Curve gh_curve)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				Point3d pointAtStart = ((GH_Goo<Curve>)(object)gh_curve).get_Value().get_PointAtStart();
				return ((Point3d)(ref pointAtStart)).DistanceTo(((GH_Goo<Curve>)(object)gh_curve).get_Value().get_PointAtEnd());
			});
		}
		List<Point3> in_nodes = FromGH.Values((IReadOnlyList<GH_Point>)list2);
		List<Vector3> in_z_oris = FromGH.Values((IReadOnlyList<GH_Vector>)list3);
		List<string> in_ids = FromGH.Values((IReadOnlyList<GH_String>)list4);
		List<Color> in_colors = FromGH.Values((IReadOnlyList<GH_Colour>)list5);
		List<CroSec> in_crosecs = FromGH.Values((IReadOnlyList<GH_CrossSection>)list6);
		if (num < 0.0)
		{
			((GH_ActiveObject)this).AddRuntimeMessage((GH_RuntimeMessageLevel)10, "The way to generate zero length elements has changed from Karamba3D 1.2.2 to the current version. See input-plug 'Points' or the manual for details.");
		}
		LineToBeam.solve(in_nodes, in_lines, new_nodes, remove_dup, num, in_z_oris, in_ids, in_colors, in_crosecs, value, bklLens, out var out_points, out var out_beams, out var info);
		DA.SetDataList(0, (IEnumerable)ToGH.Values(out_beams));
		DA.SetDataList(1, (IEnumerable)ToGH.Values(out_points));
		DA.SetData(2, (object)new GH_String(info));
 
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
            get { return new Guid("873af011-f8c1-4295-ae0a-50676ec0447a"); }
        }
    }*/
}