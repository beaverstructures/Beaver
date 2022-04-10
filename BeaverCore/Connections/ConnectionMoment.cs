using System;
using System.Collections.Generic;
using System.Text;
using BeaverCore.Geometry;
using BeaverCore.Actions;
using BeaverCore.Misc;

namespace BeaverCore.Connections
{
    [Serializable]
    public class ConnectionMoment : Connection
    {
        public ShearSpacing spacing;
        public Point2D CR = new Point2D();
        public double sumXsq = 0;
        public double sumYsq = 0;
        public double sumDsq = 0;
        public double nef_x; /// axial longitudinal
        public double nef_y; /// horizontal transversal
        public double nef_z; /// vertical transversal
        public double shearplanes;
        int SC;

        public ConnectionMoment() { }
        public ConnectionMoment(
            T2TCapacity capacity,
            Force force,
            Plane plane,
            List<Point2D> points,
            ShearSpacing spacing,
            int SC) 
        {
            fastenerCapacity = capacity;
            this.fastener = capacity.fastener;
            this.SC = SC;
            this.force = force;
            this.plane = plane;
            foreach(Point2D point in points)
            {
                this.FastenerList.Add(new FastData(point));
            }
            this.spacing = spacing;
        }

        public void Initialize()
        {
            SetProperties();
            SetFastenerForces();
            SetFastenerUtilizations();
            SetConnectionStiffness();
        }

        private void SetProperties()
        {
            double sumx = 0;
            double sumy = 0;
            foreach(FastData fastData in FastenerList)
            {
                sumx += fastData.pt.x;
                sumy += fastData.pt.y;
            }
            CR.x = sumx / FastenerList.Count;
            CR.y = sumy / FastenerList.Count;

            foreach (FastData fastData in FastenerList)
            {
                double deltaX = CR.deltaX(fastData.pt);
                double deltaY = CR.deltaY(fastData.pt);
                sumXsq += Math.Pow(deltaX, 2);
                sumYsq += Math.Pow(deltaY, 2);
                sumDsq += Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2);
            }

            nef_y = Math.Pow(spacing.npar * spacing.nperp, 0.9);
            switch (fastener.type)
            {
                case "Bolt":
                case "Dowel":
                    /// EC5 eq. 8.34
                    nef_x = spacing.nperp == 1 ? 1: Math.Min(
                        spacing.nperp*spacing.npar,
                        Math.Pow(spacing.nperp, 0.9) * Math.Pow(spacing.a1h / 13*fastener.d,0.25) * spacing.npar);
                    nef_z = spacing.npar == 1 ? 1 : Math.Min(
                        spacing.npar*spacing.nperp,
                        Math.Pow(spacing.npar, 0.9) * Math.Pow(spacing.a1v / 13 * fastener.d, 0.25) * spacing.nperp);
                    break;
                case "Screw":
                case "Nail":
                    nef_x = spacing.nperp == 1 ? 1 : GetKef(spacing.nperp, spacing.a1h)*spacing.npar;
                    nef_z = spacing.npar == 1 ? 1 : GetKef(spacing.npar, spacing.a1v)*spacing.nperp;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private double GetKef(double n,double a1)
        {
            /// EC5 SECTION 8.3.8
            double d = fastener.d;
            if (fastener.d > 0.006)
            {

                double kef = 0;
                switch (a1)
                {
                    case double space when space < 4 * d:
                        throw new ArgumentException("Spacing cannot be lower than 4x the fastener diameter");
                    case double space when space >= 4 * d & space < 7 * d:
                        if(fastener.predrilled1 & fastener.predrilled2)
                        {
                            kef = 0.5 - (0.5 - 0.7) * (4 * d - a1) / (4 * d - 7 * d); break;
                        }
                        else throw new ArgumentException("Fastener needs to be predrilled");
                    case double space when space >= 7 * d & space < 10 * d:
                        kef = 0.7 - (0.7 - 0.85) * (7 * d - a1) / (7 * d - 10 * d); break;
                    case double space when space >= 10 * d & space < 14 * d:
                        kef = 0.85 - (0.85 - 1) * (10 * d - a1) / (10 * d - 14 * d); break;
                    case double space when space >= 14 * d:
                        kef = 1; break;
                }
                return Math.Pow(n, kef);
            }
            else throw new ArgumentException("Fastener diameter cannot be lower than 6mm");
        }

        private void SetFastenerForces()
        {
            foreach (FastData fD in FastenerList)
            {
                fD.forces["Fx"] = new Vector3D(1,0,0)                                   * (force.N / nef_x);
                fD.forces["Fz"] = new Vector3D(0,1,0)                                   * (force.Vz / nef_z);
                fD.forces["Fy"] = new Vector3D(0,0,1)                                   * (force.Vy / nef_y);

                fD.forces["Fi_My"] = new Vector3D(CR.y - fD.pt.y, -CR.x + fD.pt.x, 0).Unit()* (force.My * CR.Distance(fD.pt) / sumDsq);
                fD.forces["Fi_Mz"] = new Vector3D(0,0,1)           * (force.Mz * CR.deltaX(fD.pt) / sumXsq);
                fD.forces["Fi_Mt"] = new Vector3D(0,0,1)           * (force.Mt * CR.deltaY(fD.pt) / sumYsq);

                fD.forces["Fvd"] = fD.forces["Fx"] + fD.forces["Fz"] + fD.forces["Fi_My"];      /// Vectorial sum
                fD.forces["Faxd"] = fD.forces["Fy"] + fD.forces["Fi_Mz"] + fD.forces["Fi_Mt"];  /// Vectorial sum
                fD.forces["Combined"] = fD.forces["Faxd"] + fD.forces["Fvd"];                   /// Vectorial sum
            }
        }

        private void SetFastenerUtilizations()
        {
            // *** kmod is only calculated at the end
            double kmod = Utils.KMOD(SC, force.duration);
            fastener.Fv_Rd = fastener.Fv_Rd * kmod;
            fastener.Fax_Rd = fastener.Fax_Rd * kmod;

            foreach (FastData fD in FastenerList)
            {
                

                fD.utilization["N"] = fD.forces["Fx"].Magnitude() / fastener.Fv_Rd;
                fD.utilization["Vz"] = fD.forces["Fz"].Magnitude() / fastener.Fv_Rd;
                fD.utilization["Vy"] = fD.forces["Fy"].Magnitude() / fastener.Fv_Rd;
                fD.utilization["Shear"] = fD.forces["Fvd"].Magnitude() / fastener.Fv_Rd;
                fD.utilization["Axial"] = fD.forces["Faxd"].Magnitude() / fastener.Fax_Rd;
                if(fastener.type == "Nail" & fastener.smooth == true)
                {
                    fD.utilization["Combined"] = fD.utilization["Axial"] + fD.utilization["Shear"];
                }
                else
                {
                    fD.utilization["Combined"] = Math.Pow(fD.utilization["Axial"], 2) + Math.Pow(fD.utilization["Shear"], 2);
                }
                fD.critical_utilization = GetMaxTuple(fD.utilization);
                
            }
        }

        private void SetConnectionStiffness()
        {
            double kser = fastenerCapacity.kser;
            double kdef = fastenerCapacity.kdef;
            translationalStiffness = shearplanes * kser * FastenerList.Count / (1 + kdef);
            rotationalStiffness = shearplanes * kser * sumDsq / (1 + kdef);
        }

        public Tuple<string, double> GetMaxTuple(Dictionary<string, double> keyValuePairs)
        {
            double max = 0;
            string key = null;
            foreach(KeyValuePair<string,double> keyValuePair in keyValuePairs)
            {
                if(max <= keyValuePair.Value)
                {
                    key = keyValuePair.Key;
                    max = keyValuePair.Value;
                }
            }
            return new Tuple<string, double>(key, max);
        }
    }
}
