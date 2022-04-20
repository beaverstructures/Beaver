using BeaverCore.Actions;
using BeaverCore.CrossSection;
using BeaverCore.Misc;
using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeaverCore.Geometry;

namespace BeaverCore.Frame

{
    public enum SpanType
    {
        Span,
        CantileverSpan
    }

    /// <summary>
    /// a TimberFrame element for calculating stresses and displacements on a given element
    /// </summary>
    [Serializable]
    public class TimberFrame
    {
        /// <summary>
        /// Mapping between TimberFramePoints and it's
        /// relative positions [0,1].
        /// </summary>
        public Dictionary<double, TimberFramePoint> TimberPointsMap;

        /// <summary>
        /// Geometric representation of the member axis.
        /// </summary>
        public Line FrameAxis;
        public string id;
        public SpanLine spanLine;

        [Serializable]
        public class SpanLine
        {
            public Polyline geom;
            public List<Displacement> startDisp;
            public List<Displacement> endDisp;
            public Line refLine;
            public List<Displacement> midDisp;

            public SpanLine() { }
            public SpanLine(Polyline poly)
            {
                geom = poly;
                startDisp = new List<Displacement>();
                endDisp = new List<Displacement>();
                midDisp = new List<Displacement>();
            }
        }

        public TimberFrame()
        {

        }

        public TimberFrame(Dictionary<double, TimberFramePoint> timberpoints)
        {
            TimberPointsMap = new Dictionary<double, TimberFramePoint>(timberpoints);
        }

        public TimberFrame(Dictionary<double, TimberFramePoint> timberpoints, Line line)
        {
            TimberPointsMap = new Dictionary<double, TimberFramePoint>(timberpoints);
            FrameAxis = line;
            spanLine = new SpanLine(line);
        }
        public TimberFrame(Dictionary<double, TimberFramePoint> timberpoints, Line line,SpanLine _spanLine)
        {
            TimberPointsMap = new Dictionary<double, TimberFramePoint>(timberpoints);
            FrameAxis = line;
            spanLine = _spanLine;
        }
    }

    

    public class TimberFrameULSResult
    {
        public List<string[]> ULSReport { get; }
        public List<double[]> UtilsY { get; }
        public List<double[]> UtilsZ { get; }
        public string SectionData { get; }

        public TimberFrameULSResult(List<string[]> allULSinfo) { }

        public TimberFrameULSResult(List<string[]> ulsreport, List<double[]> utilsY, List<double[]> utilsZ, string sectiondata)
        {
            ULSReport = ulsreport;
            UtilsY = utilsY;
            UtilsZ = utilsZ;
            SectionData = sectiondata;
        }
    }

    public class TimberFrameSLSResult
    {
        public string[] Info;
        public List<double> InstUtils;
        public List<double> NetFinUtils;
        public List<double> FinUtils;

        public TimberFrameSLSResult() { }

        public TimberFrameSLSResult(string[] info, List<double> instUtils, List<double> netFinUtils, List<double> finUtils)
        {
            Info = info;
            InstUtils = instUtils;
            NetFinUtils = netFinUtils;
            FinUtils = finUtils;
        }
    }
}
