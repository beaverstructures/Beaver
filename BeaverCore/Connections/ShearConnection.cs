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
        public List<Force> connection_forces;
        public List<FastenerForce[]> fastener_forces;
        public List<SingleFastenerCapacity> fastener_capacities;
        ShearSpacing spacing;
        ULSCombinations ULScombinations;
        Fastener fastener;


        public ShearConnection(List<Point2D> fastener_coordinates,List<Force>conn_force, Fastener fastener, int service_class)
        {
            this.fastener_coordinates = fastener_coordinates;
            connection_forces = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
        }

        public ShearConnection(List<Force> conn_force, Fastener fastener, int service_class)
        {
            this.fastener_coordinates = new List<Point2D>();
            connection_forces = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
        }

        public ShearConnection()
        { }

        public ShearConnection(List<Point2D> fastener_coordinates, List<Force> conn_force, Fastener fastener, int service_class,ShearSpacing spacing)
        {
            this.fastener_coordinates = fastener_coordinates;
            connection_forces = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
            this.spacing = spacing;
        }

        public ShearConnection(List<Force> conn_force, Fastener fastener, int service_class, ShearSpacing spacing)
        {
            this.fastener_coordinates = new List<Point2D>();
            connection_forces = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
            this.spacing = spacing;
        }

        public void DefineCapacities()
        {
            foreach (Point2D point in fastener_coordinates)
               }
        }

        public MultipleShearFastenerCapacity ConnectionCapacity()
        {
            MultipleShearFastenerCapacity capacity = new MultipleShearFastenerCapacity(fastener_capacities, spacing);
            return capacity;
        }

    }
}
