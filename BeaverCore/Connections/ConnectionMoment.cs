using System;
using System.Collections.Generic;
using System.Text;
using BeaverCore.Geometry;

namespace BeaverCore.Connections
{
    public class ConnectionMoment : Connection
    {
        public ShearSpacing spacing;
        public Point2D CR;
        public double sumXsq = 0;
        public double sumYsq = 0;
        public double sumDsq = 0;
        public double nef_x; /// axial longitudinal
        public double nef_y; /// horizontal transversal
        public double nef_z; /// vertical transversal
        public double shearplanes;

        public void SetProperties()
        {
            double sumx = 0;
            double sumy = 0;
            foreach(FastData fastData in FastenerList)
            {
                sumx = fastData.pt.x;
                sumy = fastData.pt.y;
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
            if (fastener.d < 6)
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

        public void SetFastenerForces()
        {
            foreach(FastData fD in FastenerList)
            {
                fD.force.Fx     = new Vector3D(plane.U).Unit()                          * (force.N / nef_x);
                fD.force.Fz     = new Vector3D(plane.V).Unit()                          * (force.Vz / nef_z);
                fD.force.Fy     = new Vector3D(plane.U.CrossProduct(plane.V)).Unit()    * (force.Vy / nef_y);
                fD.force.Fi_My  = new Vector3D(CR.y - fD.pt.y, -CR.x + fD.pt.x,0).Unit()* (force.My * CR.Distance(fD.pt) / sumDsq);
                fD.force.Fi_Mz  = new Vector3D(plane.U.CrossProduct(plane.V))           * (force.Mz * CR.deltaX(fD.pt) / sumXsq);
                fD.force.Fi_Mt  = new Vector3D(plane.U.CrossProduct(plane.V))           * (force.Mt * CR.deltaY(fD.pt) / sumYsq);

                fD.force.Fvd = fD.force.Fx + fD.force.Fz + fD.force.Fi_My;      /// Vectorial sum
                fD.force.Faxd = fD.force.Fy + fD.force.Fi_Mz + fD.force.Fi_Mt;  /// Vectorial sum
            }
        }
    }
}
