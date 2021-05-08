using BeaverCore.Actions;
using BeaverCore.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;

namespace BeaverCore.Connections
{
    public class ShearConnection
    {
        public List<Point2D> fastener_coordinates;
        public List<Force> connection_force;
        ULSCombinations ULScombinations;
        Fastener fastener;


        public ShearConnection(List<Point2D> fastener_coordinates,List<Force>conn_force, Fastener fastener, int service_class)
        {
            this.fastener_coordinates = fastener_coordinates;
            connection_force = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
        }

        public ShearConnection(List<Force> conn_force, Fastener fastener, int service_class)
        {
            this.fastener_coordinates = new List<Point2D>();
            connection_force = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
        }

        public ShearConnection()
        {
        }

        public MultipleShearFastenerCapacity ConnectionCapacity()
        {
            throw new NotImplementedException();
        }
    }
}
