using BeaverCore.CrossSection;
using BeaverCore.Frame;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BeaverCore.Actions;
using BeaverCore.Misc;
using static BeaverGrasshopper.Components.ResultsComponents.ExtendedMethods;

namespace BeaverGrasshopper.Components.ResultsComponents
{
    public class TimberFrameView : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the TimberFrameView class.
        /// </summary>
        public TimberFrameView()
          : base("TimberFrameView", "Nickname",
              "Description",
              "Beaver", "4.Results")
        {
        }

        List<Mesh> meshes = new List<Mesh>();
        ULSCombinations uls_comb = new ULSCombinations();
        SLSCombinations sls_comb = new SLSCombinations();
        bool value_list_updated = false;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_TimberFrame(), "Timber Frames", "TF's", "Timber Frames to visualize", GH_ParamAccess.list);
            pManager.AddTextParameter("Utilization Type", "UType", "Utilization Type to Preview", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_TimberFrame(), "Timber Frames", "TF's", "Modified timber frames", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.list);
            pManager.AddTextParameter("Legend T", "T", "Legend T", GH_ParamAccess.list);
            pManager.AddColourParameter("Legend C", "C", "Legend C", GH_ParamAccess.list);
            pManager.AddNumberParameter("Maximum Utilization", "MaxUtil", "Maximum Utilization", GH_ParamAccess.item);
        }
        UtilizationType util_type = UtilizationType.All;
        SLSOptions sls_options = SLSOptions.Inst;
        ULSDirection uls_dir = ULSDirection.All;
        bool update_field_2 = false;
        bool update_field_3 = false;
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
                foreach (UtilizationType util_type in Enum.GetValues(typeof(UtilizationType)))
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


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<Color> colors = new List<Color>() {
                Color.Black,
                Color.FromArgb(165, 0, 38), 
                Color.FromArgb(215,48,39),
                Color.FromArgb(244,109,67), 
                Color.FromArgb(253,174,97),
                Color.FromArgb(254,224,144), 
                Color.FromArgb(255,255,191),
                Color.FromArgb(224,243,248), 
                Color.FromArgb(171,217,233),
                Color.FromArgb(116,173,209), 
                Color.FromArgb(69,117,180),
                Color.FromArgb(49,54,149) };
            colors.Reverse();
            List<String> legend = new List<String>() {
                "0%","10%","20%","30%","40%","50%","60%","70%","80%","90%","100%",">100%"
            };

            meshes.Clear();
            List<GH_TimberFrame> gh_timber_frames = new List<GH_TimberFrame>();
            string type = "";
            string sls_type_string = "";
            string uls_dir_string = "";
            string loadcase_string = "";
            int loadcase_index = -1;

            DA.GetDataList(0, gh_timber_frames);
            DA.GetData(1, ref type);

            uls_comb = gh_timber_frames[0].Value.TimberPointsMap[0].ULSComb;
            sls_comb = gh_timber_frames[0].Value.TimberPointsMap[0].SLSComb;
            
            UtilizationType new_util_type = (UtilizationType)Enum.Parse(typeof(UtilizationType), type, true);
            if (Update_util(util_type, new_util_type))
            {
                update_field_2 = true;
                update_field_3 = true;
                util_type = new_util_type;
                return;
            }
            else
            {
                util_type = new_util_type;
                if (Params.Input.Count == 4)
                {
                    if (util_type == UtilizationType.SLS)
                    {
                        DA.GetData(2, ref sls_type_string);
                        SLSOptions new_sls_options = (SLSOptions)Enum.Parse(typeof(SLSOptions), sls_type_string, true);
                        if (new_sls_options != sls_options) { sls_options = new_sls_options; update_field_3 = true; return; }
                    }
                    else
                    {
                        DA.GetData(2, ref uls_dir_string);
                        ULSDirection new_uls_dir = (ULSDirection)Enum.Parse(typeof(ULSDirection), uls_dir_string, true);
                        uls_dir = new_uls_dir;
                    }
                    DA.GetData(3, ref loadcase_string);
                    try
                    {
                        loadcase_index = Int32.Parse(loadcase_string);
                    }
                    catch
                    {
                    }
                }
            }
            double max_util = 0;
            List<GH_TimberFrame> out_beams = new List<GH_TimberFrame>();
            foreach (GH_TimberFrame gh_timber_frame in gh_timber_frames)
            {
                TimberFrame timber_frame = gh_timber_frame.Value;
                Mesh mesh = timber_frame.MeshfromTimberFrame(
                    util_type, 
                    uls_dir, 
                    sls_options, 
                    loadcase_index, 
                    ref max_util,
                    colors,
                    "Utilization");
                meshes.Add(mesh);
                out_beams.Add(new GH_TimberFrame(timber_frame));
            }

            DA.SetDataList(0, gh_timber_frames);
            DA.SetDataList(1, meshes);
            DA.SetDataList(2, legend);
            DA.SetDataList(3, colors);
            DA.SetData(4, max_util);
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
            Param_String param = new Param_String();
            string name = "";
            string nickname = "";
            string description = "";

            if (index == 2)
            {
                if (util_type == UtilizationType.SLS)
                {
                    name = "SLS Options";
                    nickname = "Options";
                    description = "SLS Results Options";
                }
                else
                {
                    name = "ULS Direction";
                    nickname = "Direction";
                    description = "ULS Directions Options";
                }
            }
            else if (index == 3)
            {
                name = "Load Case Index";
                nickname = "LCindex";
                description = "Load Case Index to Show";
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
            
            if (Grasshopper.Instances.ActiveCanvas.Document != null)
            {
                GH_Document document = Grasshopper.Instances.ActiveCanvas.Document;
                if (util_type != UtilizationType.All)
                {
                    if (update_field_2)
                    {
                        if (Params.Input.Count > 2)
                        {
                            foreach (var source in Params.Input[3].Sources)
                            {
                                document.RemoveObject(source, true);
                            }
                            foreach (var source in Params.Input[2].Sources)
                            {
                                document.RemoveObject(source, true);
                            }
                            Params.Input[3].Sources.Clear();
                            Params.UnregisterInputParameter(Params.Input[3]);
                            Params.Input[2].Sources.Clear();
                            Params.UnregisterInputParameter(Params.Input[2]);
                        }
                        Params.OnParametersChanged();
                        if (Params.Input.Count == 2) Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 2));
                        if (util_type == UtilizationType.SLS) AddValueListforSLSOptions(this);
                        else AddValueListforULSDirection(this);
                        Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 3));
                        AddValueListforLoadCase(this);
                        Params.OnParametersChanged();
                        PointF currPivot = Params.Input[1].Attributes.Pivot;
                        //set the pivot of the new object
                        Params.Input[1].Sources[0].Attributes.Pivot = new PointF(currPivot.X - 250, currPivot.Y - 14);
                        update_field_2 = false;
                        update_field_3 = false;
                        Params.OnParametersChanged();
                        value_list_updated = true;
                    }
                    else if (update_field_3)
                    {
                        foreach (var source in Params.Input[3].Sources)
                        {
                            document.RemoveObject(source, true);
                        }
                        Params.Input[3].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[3]);
                        Params.OnParametersChanged();
                        Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, 3));
                        Params.OnParametersChanged();
                        AddValueListforLoadCase(this);
                        update_field_3 = false;
                        PointF currPivot = Params.Input[1].Attributes.Pivot;
                        //set the pivot of the new object
                        Params.Input[1].Sources[0].Attributes.Pivot = new PointF(currPivot.X - 250, currPivot.Y - 14);
                        Params.OnParametersChanged();
                        value_list_updated = true;
                    }
                }
                else
                {
                    if (Params.Input.Count > 2)
                    {
                        foreach (var source in Params.Input[3].Sources)
                        {
                            document.RemoveObject(source, true);
                        }
                        foreach (var source in Params.Input[2].Sources)
                        {
                            document.RemoveObject(source, true);
                        }
                        Params.OnParametersChanged();
                        Params.Input[3].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[3]);


                        Params.Input[2].Sources.Clear();
                        Params.UnregisterInputParameter(Params.Input[2]);
                        Params.OnParametersChanged();
                        PointF currPivot = Params.Input[1].Attributes.Pivot;
                        //set the pivot of the new object
                        Params.Input[1].Sources[0].Attributes.Pivot = new PointF(currPivot.X - 250, currPivot.Y + 14);
                        Params.OnParametersChanged();
                        value_list_updated = true;
                    }
                }
            }
            

        }

        void AddValueListforSLSOptions(GH_Component component)
        {
            GH_Document document = Grasshopper.Instances.ActiveCanvas.Document;
            if (Params.Input[2].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate utilization type list
                var vl = new GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "";
                //clear default contents
                vl.ListItems.Clear();
                foreach (SLSOptions sls in Enum.GetValues(typeof(SLSOptions)))
                    vl.ListItems.Add(new GH_ValueListItem(sls.ToString(), "\"" + sls.ToString() + "\""));
                document.AddObject(vl, false);
                Params.Input[2].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[2].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 180, currPivot.Y - 22);
            }
            base.AddedToDocument(document);
        }

        void AddValueListforULSDirection(GH_Component component)
        {
            GH_Document document = Grasshopper.Instances.ActiveCanvas.Document;
            if (Params.Input[2].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate utilization type list
                var vl = new GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "";
                //clear default contents
                vl.ListItems.Clear();
                foreach (ULSDirection dir in Enum.GetValues(typeof(ULSDirection)))
                    vl.ListItems.Add(new GH_ValueListItem(dir.ToString(), "\"" + dir.ToString() + "\""));
                document.AddObject(vl, false);
                Params.Input[2].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[2].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 180, currPivot.Y - 22);
            }
            base.AddedToDocument(document);
        }

        void AddValueListforLoadCase(GH_Component component)
        {
            GH_Document document = Grasshopper.Instances.ActiveCanvas.Document;
            if (Params.Input[3].SourceCount == 0)
            {
                // Perform Layout to get actual positionning of the component on the canevas
                this.Attributes.ExpireLayout();
                this.Attributes.PerformLayout();

                //instantiate utilization type list
                var vl = new GH_ValueList();
                vl.CreateAttributes();
                vl.NickName = "";
                //clear default contents
                vl.ListItems.Clear();
                int cont = -1;
                vl.ListItems.Add(new GH_ValueListItem("All", cont.ToString()));
                cont++;
                if (util_type == UtilizationType.SLS)
                {
                    List<Displacement> disps = new List<Displacement>();
                    if (sls_options == SLSOptions.Inst) disps = sls_comb.CharacteristicDisplacements;
                    else disps = sls_comb.CreepDisplacements;
                    foreach (Displacement combination in disps)
                    {
                        string comb_string = combination.combination;
                        vl.ListItems.Add(new GH_ValueListItem(comb_string, cont.ToString()));
                        cont++;
                    }
                }
                else
                {
                    List<Force> forces = new List<Force>(uls_comb.DesignForces);
                    foreach (Force combination in forces)
                    {
                        string comb_string = combination.combination;
                        vl.ListItems.Add(new GH_ValueListItem(comb_string, cont.ToString()));
                        cont++;
                    }
                }
                document.AddObject(vl, false);
                Params.Input[3].AddSource(vl);
                //get the pivot of the "accent" param
                PointF currPivot = Params.Input[3].Attributes.Pivot;
                //set the pivot of the new object
                vl.Attributes.Pivot = new PointF(currPivot.X - 180, currPivot.Y - 8);
            }
            base.AddedToDocument(document);
        }

        #endregion

        protected override void AfterSolveInstance()
        {
            if (value_list_updated)
            {
                ExpireSolution(true);
                value_list_updated = false;
            }
            VariableParameterMaintenance();
            if (value_list_updated)
            {
                ExpireSolution(true);
            }

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
                return Properties.Resources.TimberFrameResults;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1d376903-8946-41bc-83f2-0ce50fe2ff61"); }
        }
    }
}