using Grasshopper.Kernel;
using Grasshopper.GUI;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;

using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;

using BeaverGrasshopper.Components.InteropComponents;
using BeaverCore.Frame;
using BeaverCore.Geometry;

namespace BeaverGrasshopper.Components.ResultsComponents
{
    public class ViewValueResults : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ViewValueResults class.
        /// </summary>
        public ViewValueResults()
          : base("ViewNumericResults", "NumericResults",
              "Displays numerical results on timber frame",
              "Beaver", "4.Results")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_TimberFrame(), "Timber Frames", "TF's", "Timber Frames to visualize", GH_ParamAccess.list);
            pManager.AddTextParameter("Value type", "Type", "Results to be displayed. Accepted values are 'Utilization' or 'Critical Check'", GH_ParamAccess.item,"Utilization") ;
            pManager.AddColourParameter("Text Colour", "colour", "colour", GH_ParamAccess.item,Color.DarkGray);
            pManager.AddNumberParameter("Text Size", "Size", "Text Size", GH_ParamAccess.item,0.5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        
        string type = "";
        Color color = Color.DarkGray;
        double size = 1;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_TimberFrame> tfs = new List<GH_TimberFrame>();
            _text.Clear();
            _point.Clear();

            DA.GetDataList(0, tfs);
            DA.GetData(1, ref type);
            DA.GetData(2, ref color);
            DA.GetData(3, ref size);

            for (int i = 0;i< tfs.Count; i++)
            {
                TimberFrame timberFrame = tfs[i].Value;
                foreach(TimberFramePoint tfPoint in timberFrame.TimberPointsMap.Values)
                {
                    switch (type)
                    {
                        case "Utilization": _text.Add(Math.Round(tfPoint.util, 2).ToString()); break;
                        case "Critical Check": _text.Add(tfPoint.util_index.ToString()); break;
                        default: throw new ArgumentException("type not found.");
                    }
                    _point.Add(new Point3d(tfPoint.pt.x, tfPoint.pt.y, tfPoint.pt.z));
                }
                
            }
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                return BoundingBox.Empty;
            }
        }

        #region text tags
        private List<string> _text = new List<string>();
        private List<Point3d> _point = new List<Point3d>();
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_text.Count == 0)
                return;

            Plane plane;
            args.Viewport.GetFrustumFarPlane(out plane);

            for (int i = 0; i < _text.Count; i++)
            {
                string text = _text[i];
                Point3d point = _point[i];
                plane.Origin = point;

                Rhino.Display.Text3d drawText = new Rhino.Display.Text3d(text, plane, size);
                args.Display.Draw3dText(drawText, color);
                drawText.Dispose();
            }
        }
        #endregion

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.ViewValueResults;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b779b689-8a27-4c9c-8e2d-5af0962d5d31"); }
        }
    }
}