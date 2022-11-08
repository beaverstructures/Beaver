using System;
using System.Collections.Generic;
using System.Text;
using BeaverCore.Actions;
using BeaverCore.Connections;
using BeaverCore.CrossSection;
using BeaverCore.Frame;
using BeaverCore.Materials;
using BeaverCore.Misc;
using SpanLine = BeaverCore.Frame.TimberFrame.SpanLine;


namespace BeaverCore.Model
{
    public class BeaverModel : ICloneable
    {
        /// <summary>
        /// an assembly of all elements and the base object for performing timber analysis
        /// </summary>
        #region FIELDS
        public List<Material> materials = new List<Material>();
        public List<CroSec> crosSecs = new List<CroSec>();
        public List<TimberFrame> timberFrames = new List<TimberFrame>();
        public List<TimberFramePoint> nodes = new List<TimberFramePoint>();
        public List<TimberFramePoint> tfPts = new List<TimberFramePoint>();
        public List<ConnectionAxial> ConnectionAxials = new List<ConnectionAxial>();
        public List<SpanLine> spanLines = new List<SpanLine>();
        public ULSCombinations ULSComb = new ULSCombinations();
        public SLSCombinations SLSComb = new SLSCombinations();
        #endregion

        #region CONSTRUCTORS
        public BeaverModel() { }
        public BeaverModel(BeaverModel model)
        {
            materials = new List<Material>(model.materials.DeepClone());
            crosSecs = new List<CroSec>(model.crosSecs.DeepClone());
            timberFrames = new List<TimberFrame>(model.timberFrames.DeepClone());
            tfPts = new List<TimberFramePoint>(model.tfPts.DeepClone());
            ConnectionAxials = new List<ConnectionAxial>(model.ConnectionAxials.DeepClone());
            spanLines = new List<SpanLine>(model.spanLines.DeepClone());
            ULSComb = model.ULSComb.DeepClone();
            SLSComb = model.SLSComb.DeepClone();
        }

        public BeaverModel(
            List<Material> materials = null, 
            List<CroSec> crosSecs = null, 
            List<TimberFrame> timberFrames = null, 
            List<TimberFramePoint> tfPts = null,
            List<ConnectionAxial> ConnectionAxials = null,
            List<SpanLine> spanLines = null,
            ULSCombinations ULSComb = null,
            SLSCombinations SLSComb = null
            )
        {
            this.materials = materials;
            this.crosSecs = crosSecs;
            this.timberFrames = timberFrames;
            this.tfPts = tfPts;
            this.ConnectionAxials = ConnectionAxials;
            this.spanLines = spanLines;
            this.ULSComb = ULSComb;
            this.SLSComb = SLSComb;
        }

        public object Clone()
        {
            BeaverModel clone = new BeaverModel(this);
            return clone;
        }
        #endregion

        #region METHODS 
        public void SetCombinations()
        {
            throw new NotImplementedException();
        }
        public void SetDesignForces()
        {
            throw new NotImplementedException();
        }

        public void SetUtilizations()
        {
            throw new NotImplementedException();
        }

        public virtual void ImportK3DForces(Object obj)
        {
            throw new NotImplementedException();
        }
        public virtual void KarambaModelToBeaverModel()
        {
            throw new NotImplementedException();
        }
        public virtual void AssignSpanLines()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
