 using BeaverCore.Actions;
using BeaverCore.Geometry;
using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Connections
{
    public class ConnectionAxial : Connection
    {
        // Calculates the axial capacity of the connection
        public List<Point2D> fastener_coordinates;
        public List<Force> connection_forces;
        public AxialSpacing spacing;
        public List<FastenerForce> fastener_forces;
        public SingleFastenerCapacity fastenerCapacity;
        public Fastener fastener;
        // ConnectionType connection_type;
        ULSCombinations ULScombinations;
        int service_class;
        bool isMultiple;
        double nef;
        string cricticalFailure;
        double cricticalValue = 99999;

        public ConnectionAxial(Fastener fastener, AxialSpacing axialSpacing)
        {
            this.spacing = axialSpacing;
            this.fastener = fastener;
            this.isMultiple = (axialSpacing.npar + axialSpacing.nperp) == 1 ? true : false;
        }

        public ConnectionAxial() { }

        public void AxialResistance()
        {          

            if (isMultiple)
            {
                // Calculates nef based on Axial Spacing
                nef = Math.Pow(spacing.n, 0.9);


                foreach (string failure in fastenerCapacity.axial_capacities.Keys)
                {
                    // retrieves the crictical failure mode and its value
                    if (cricticalValue > fastenerCapacity.axial_capacities[failure])
                    {
                        cricticalValue = fastenerCapacity.axial_capacities[failure];
                        cricticalFailure = failure;
                    };
                }
            }
            else
            {
                nef = 1;
            }
        }

        
        public void SetFastenerForces()
        {
            // CALCULATES PROJECTIONS OF FORCES ON FASTENER BASED ON A LIST OF ACTING FORCES ON THE CONNECTION
            // USES LOCAL COORDINATE SYSTEMS OF TIMBERFRAME ELEMENT
            foreach (Force force in connection_forces)
            {
                Vector2D forceVector = new Vector2D(force.N, force.Vz);
                Vector2D FaxdVector = (forceVector.DotProduct(fastener.vector) /
                                        Math.Pow(fastener.vector.Magnitude(), 2)
                                        * fastener.vector);             // LINEAR ALGEBRA: PROJECTION OF FORCES ON FASTENER
                double Faxd =   FaxdVector.Magnitude() 
                                / spacing.n;       
                double Fvd =    (forceVector - FaxdVector).Magnitude()  // LINEAR ALGEBRA: ORTHOGONAL PROJECTION OF FORCE ON FASTENER
                                / spacing.n;
                fastener_forces.Add(new FastenerForce(Faxd, Fvd, fastener.alpha, force.duration));
            }
        }

        public void SetFastenerCapacity()
        {
            // Finds the design resistance of fastener based on AxialSpacing and Fastener properties
            
            throw new NotImplementedException();
        }


        public class AxialConnectionUtilization
        {
            List<List<double>> utilization = new List<List<double>>();
            List<List<string>> failure_mode = new List<List<string>>();
        }
    }
}
