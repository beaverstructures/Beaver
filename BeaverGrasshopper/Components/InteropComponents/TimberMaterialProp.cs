using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Karamba.GHopper.Materials;
using Karamba.GHopper.Utilities;
using Karamba.GHopper.Utilities.UIWidgets;
using Karamba.Properties;

public class Component_MaterialProps : GH_SwitcherComponent
{
    ///$$$ dont know if i have to change something here
	
	private List<SubComponent> subcomponents_ = new List<SubComponent>();

	public override string UnitMenuName => "Material Type:";

	protected override string DefaultEvaluationUnit => subcomponents_[0].name();

	public override Guid ComponentGuid => new Guid("{F66012C9-4F86-4720-8B2A-E290ED630260}");

	public override GH_Exposure Exposure => (GH_Exposure)2;

	protected override Bitmap Icon => Resources.MaterialProperties;
	
	/// <summary>
	/// Initializes a new instance of the MyComponent1 class.
	/// </summary>
	public Component_MaterialProps()
		: base("Timber Material Properties (Beaver to Karamba3D)", "TimberProps", 
			"Sets the characteristic timber parameters to allow further Beaver ULS and SLS analysis.", "Category", "Subcategory")
	{
		((GH_Component)this).set_Hidden(true);
	}

	/// <summary>
	/// Registers all the input parameters for this component.
	/// </summary>
	protected override void RegisterInputParams(GH_InputParamManager pManager)
	{
		pManager.AddTextParameter("Family of material", "Family", "Family of the material. Can be 'steel', 'aluminum', 'wood' or 'concrete'.", (GH_ParamAccess)0, "Steel");
		((GH_Component)this).get_Params().get_Input()[0].set_Optional(true);
		pManager.AddTextParameter("Name", "Name", "Name of the material.", (GH_ParamAccess)0, "S235");
		((GH_Component)this).get_Params().get_Input()[1].set_Optional(true);
		pManager.AddGenericParameter("Element|Identifier", "Elem|Id", "Identifier of beam or beam with identifier to attach the material to.", (GH_ParamAccess)0);
		((GH_Component)this).get_Params().get_Input()[2].set_Optional(true);
		pManager.AddColourParameter("Color", "Color", "Colour of the material.", (GH_ParamAccess)0);
		((GH_Component)this).get_Params().get_Input()[3].set_Optional(true);
	}

	/// <summary>
    /// I think this must go since no option for ortho vs isotropic will be available
	/*
	{
		subcomponents_.Add(new SubComponent_MaterialPropsIsotrop());
		subcomponents_.Add(new SubComponent_MaterialPropsOrthotropic());
		foreach (SubComponent item in subcomponents_)
		{
			item.registerEvaluationUnits(mngr);
		}
	}

	protected override void OnComponentLoaded()
	{
		base.OnComponentLoaded();
		foreach (SubComponent item in subcomponents_)
		{
			item.OnComponentLoaded();
		}
	}

	protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
	{
		((GH_Component)this).AppendAdditionalComponentMenuItems(menu);
		GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Expand ValueLists", (EventHandler)ExpandValueLists);
	}

	private void ExpandValueLists(object sender, EventArgs e)
	{
		if (activeUnit.Name.Equals("Isotrop"))
		{
			ValueListUtils.expandValueList((GH_Component)(object)this, 11, (GH_ValueListMode)1);
			((GH_DocumentObject)this).ExpireSolution(true);
		}
		else if (activeUnit.Name.Equals("Orthotropic"))
		{
			ValueListUtils.expandValueList((GH_Component)(object)this, 19, (GH_ValueListMode)1);
			((GH_DocumentObject)this).ExpireSolution(true);
		}
	}

	protected override void BeforeSolveInstance(EvaluationUnit unit)
	{
		foreach (SubComponent item in subcomponents_)
		{
			if (unit.Name.Equals(item.name()))
			{
				item.BeforeSolveInstance((GH_Component)(object)this);
				break;
			}
		}
	}
	*/

    /// protected override void RegisterOutputParams(GH_OutputParamManager pManager)
	{
		pManager.RegisterParam((IGH_Param)(object)new Param_FemMaterial(), "Material", "Mat", "Material of an element");
	}

	
    /// 
    /// </summary>
    /// <param name="pManager"></param>
	

	/// <summary>
	/// This is the method that actually does the work.
	/// </summary>
	/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
	protected override void SolveInstance(IGH_DataAccess DA, EvaluationUnit unit)
	{
		msg = "";
		level = (GH_RuntimeMessageLevel)10;
		UnitsConversionFactory unitsConversionFactory = UnitsConversionFactory.Conv();
		UnitConversion unitConversion = unitsConversionFactory.kN_cm2();
		UnitConversion unitConversion2 = unitsConversionFactory.kN_m3();
		UnitConversion unitConversion3 = unitsConversionFactory._dT();
		GH_String val = new GH_String("Steel");
		DA.GetData<GH_String>(0, ref val);
		GH_String val2 = new GH_String("S235");
		DA.GetData<GH_String>(1, ref val2);
		List<string> beamIDs = new List<string>();
		IGH_Goo in_gh_target_elem = (IGH_Goo)new GH_String("");
		DA.GetData<IGH_Goo>(2, ref in_gh_target_elem);
		GHUtils.readElementIDs(in_gh_target_elem, out beamIDs);
		GH_Colour val3 = new GH_Colour();
		Color? color = null;
		if (DA.GetData<GH_Colour>(3, ref val3))
		{
			color = ((GH_Goo<Color>)(object)val3).get_Value();
		}
		double num = unitConversion.toUnit(21000.0);
		DA.GetData<double>(4, ref num);
		num = unitConversion.toBase(num);
		double num2 = unitConversion.toUnit(21000.0);
		DA.GetData<double>(5, ref num2);
		num2 = unitConversion.toBase(num2);
		double num3 = unitConversion.toUnit(8076.0);
		DA.GetData<double>(6, ref num3);
		num3 = unitConversion.toBase(num3);
		double nue = -1.0;
		DA.GetData<double>(7, ref nue);
		double num4 = unitConversion.toUnit(8076.0);
		DA.GetData<double>(8, ref num4);
		num4 = unitConversion.toBase(num4);
		double num5 = unitConversion.toUnit(8076.0);
		DA.GetData<double>(9, ref num5);
		num5 = unitConversion.toBase(num5);
		double num6 = unitConversion2.toUnit(78.5);
		DA.GetData<double>(10, ref num6);
		num6 = unitConversion2.toBase(num6);
		double num7 = unitConversion3.toUnit(1E-05);
		DA.GetData<double>(11, ref num7);
		num7 = unitConversion3.toBase(num7);
		double num8 = unitConversion3.toUnit(1E-05);
		DA.GetData<double>(12, ref num8);
		num8 = unitConversion3.toBase(num8);
		double num9 = unitConversion.toUnit(23.5);
		DA.GetData<double>(13, ref num9);
		num9 = unitConversion.toBase(num9);
		double num10 = unitConversion.toUnit(23.5);
		DA.GetData<double>(14, ref num10);
		num10 = unitConversion.toBase(num10);
		double num11 = unitConversion.toUnit(-23.5);
		DA.GetData<double>(15, ref num11);
		num11 = unitConversion.toBase(num11);
		double num12 = unitConversion.toUnit(-23.5);
		DA.GetData<double>(16, ref num12);
		num12 = unitConversion.toBase(num12);
		double num13 = unitConversion.toUnit(13.6);
		DA.GetData<double>(17, ref num13);
		num13 = unitConversion.toBase(num13);
		double num14 = unitConversion.toUnit(0.0);
		DA.GetData<double>(18, ref num14);
		num14 = unitConversion.toBase(num14);
		GH_String val4 = new GH_String("MISES");
		DA.GetData<GH_String>(19, ref val4);
		FemMaterial.FlowHypothesis flowHypo = FemMaterial.FlowHypothesisFromString(((GH_Goo<string>)(object)val4).get_Value());
		FemMaterial femMaterial = null;
		
		try
		{
			femMaterial = new FemMaterial_Orthotropic(((GH_Goo<string>)(object)val).get_Value(), ((GH_Goo<string>)(object)val2).get_Value(), num, num2, num3, nue, num4, num5, num6, num9, num10, num11, num12, num13, num14, flowHypo, num7, num8, color);
			foreach (string item in beamIDs)
			{
				femMaterial.AddBeamId(item);
			}
		}
		catch (Exception ex)
		{
			level = (GH_RuntimeMessageLevel)20;
			msg = "Material #" + DA.get_Iteration() + " " + ex.Message;
			return;
		}

		DA.SetData(0, (object)new GH_FemMaterial(femMaterial));


		if (unit == null)
		{
			return;
		}
		foreach (SubComponent item in subcomponents_)
		{
			if (unit.Name.Equals(item.name()))
			{
				item.SolveInstance(DA, out var msg, out var level);
				if (msg != "")
				{
					((GH_ActiveObject)this).AddRuntimeMessage(level, msg + " May cause errors in exported models.");
				}
				return;
			}
		}
		throw new Exception("Invalid sub-component");
	}
}
