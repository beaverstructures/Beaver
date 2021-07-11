using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Connections
{
    class ConnectionAxialCapacity
    {
        // Calculates the axial capacity of the connection

        public SingleFastenerCapacity fastenerCapacity;
        public Fastener fastener;
        public AxialSpacing spacing;
        bool isMultiple;
        double nef;
        string cricticalFailure;
        double cricticalValue = 99999;

        public ConnectionAxialCapacity(SingleFastenerCapacity fastenerCapacity, Fastener fastener, AxialSpacing axialSpacing)
        {
            this.fastenerCapacity = fastenerCapacity;
            this.spacing = axialSpacing;
            this.fastener = fastener;
            this.isMultiple = (axialSpacing.npar + axialSpacing.nperp) == 1 ? true : false;
        }

        public void AxialResistance()
        {          

            if (isMultiple)
            {
                // Calculates nef based on Axial Spacing
                nef = Math.Pow(spacing.npar * spacing.nperp, 0.9);


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

            }
        }
    }
}
