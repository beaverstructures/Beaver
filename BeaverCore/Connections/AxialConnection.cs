using BeaverCore.Actions;
using BeaverCore.Geometry;
using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using BeaverCore.Misc;

namespace BeaverCore.Connections
{
    public class AxialConnection
    {
        public List<Point2D> fastener_coordinates;
        public List<Force> connection_forces;
        public List<Force> fastener_forces;
        public List<MultipleShearFastenerCapacity> fastener_capacities;
        ConnectionType connection_type;
        AxialSpacing spacing;
        ULSCombinations ULScombinations;
        int service_class;

        Fastener fastener;

        public Force GetForcesOnFastener()
        {
            Force Fsd = new Force();

            return Fsd;
        }

        public class AxialConnectionUtilization
        {
            List<List<double>> utilization = new List<List<double>>();
            List<List<string>> failure_mode = new List<List<string>>();
        }
    }
}
