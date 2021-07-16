using BeaverCore.Actions;
using BeaverCore.Geometry;
using BeaverCore.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using BeaverCore.Misc;
using BeaverCore.Connections;

namespace BeaverCore.Connections
{

    public enum ConnectionType
    {
        TimbertoTimber,
        TimbertoSteel
    }
    [Serializable]
    public class ShearConnection
    {
        public List<Point2D> fastener_coordinates;
        public List<Force> connection_forces;
        public List<List<FastenerForce>> fastener_forces;
        public List<ConnectionShearFastenerCapacity> fastener_capacities;
        ConnectionType connection_type;
        ShearSpacing spacing;
        ULSCombinations ULScombinations;
        int service_class;

        Fastener fastener;

        public ShearConnection(List<Point2D> fastener_coordinates, Fastener fastener, int service_class, ConnectionType connection_type = ConnectionType.TimbertoTimber)
        {
            this.fastener_coordinates = fastener_coordinates;
            this.fastener = fastener;
            this.connection_type = connection_type;
            DefineCapacities();
        }
        public ShearConnection(List<Point2D> fastener_coordinates, List<Force> conn_force, Fastener fastener, int service_class, ConnectionType connection_type = ConnectionType.TimbertoTimber)
        {
            this.fastener_coordinates = fastener_coordinates;
            connection_forces = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
            this.connection_type = connection_type;
            DefineCapacities();
        }

        public ShearConnection(List<Force> conn_force, Fastener fastener, int service_class, ConnectionType connection_type = ConnectionType.TimbertoTimber)
        {
            this.fastener_coordinates = new List<Point2D>();
            connection_forces = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
            this.connection_type = connection_type;
            DefineCapacities();
        }

        public ShearConnection()
        { }

        public ShearConnection(List<Point2D> fastener_coordinates, List<Force> conn_force, Fastener fastener, int service_class, ShearSpacing spacing, ConnectionType connection_type = ConnectionType.TimbertoTimber)
        {
            this.fastener_coordinates = fastener_coordinates;
            connection_forces = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
            this.spacing = spacing;
            this.connection_type = connection_type;
            DefineCapacities();
        }

        public ShearConnection(List<Force> conn_force, Fastener fastener, int service_class, ShearSpacing spacing, ConnectionType connection_type = ConnectionType.TimbertoTimber)
        {
            this.fastener_coordinates = new List<Point2D>();
            connection_forces = conn_force;
            ULScombinations = new ULSCombinations(conn_force, service_class);
            this.fastener = fastener;
            this.spacing = spacing;
            this.connection_type = connection_type;
            DefineCapacities();
        }

        public List<Vector2D> UnitMomentForces()
        {
            List<Vector2D> unit_moment_forces = new List<Vector2D>();
            double sumr2 = 0;
            foreach (Point2D pt in fastener_coordinates)
            {
                double r2 = Math.Pow(pt.x, 2) + Math.Pow(pt.y, 2);
                sumr2 += r2;
            }
            for (int i = 0; i < fastener_coordinates.Count; i++)
            {
                Vector2D r = Vector2D.fromPoint(fastener_coordinates[i]);
                Vector2D unit_force = r.RotatedVector(0.5 * Math.PI).Unit();
                unit_force = (r.Magnitude() / sumr2) * unit_force;
                unit_moment_forces.Add(unit_force);
            }

            return unit_moment_forces;
        }

        public void DefineCapacities()
        {
            List<Vector2D> unit_moment_forces = UnitMomentForces();
            Vector2D x_vector = new Vector2D(1, 0);
            Vector2D y_vector = new Vector2D(0, 1);
            bool timber_to_timber = connection_type == ConnectionType.TimbertoTimber;
            foreach (Force force in ULScombinations.DesignForces)
            {
                List<SingleFastenerCapacity> single_capacities = new List<SingleFastenerCapacity>();
                List<FastenerForce> forces = new List<FastenerForce>();
                foreach (Vector2D unit_force in unit_moment_forces)
                {
                    Vector2D fastener_force_vector = unit_force * force.My + x_vector * force.N + y_vector * force.Vz;
                    double alpha = Math.Abs(fastener_force_vector.AngletoVector(x_vector));
                    double fastener_force = fastener_force_vector.Magnitude();
                    //TODO: Add ConnectionProperties class to simplify constructor
                    if (timber_to_timber) single_capacities.Add(new T2TCapacity(fastener, true, true, new Material(), new Material(), alpha, alpha, alpha, 10, 10));

                    else single_capacities.Add(new T2SCapacity(fastener, false, 500, alpha, alpha, new Material(), 10, 2, 0.1, 2, SteelPosition.SteelIn));

                    forces.Add(new FastenerForce(fastener_force_vector.Magnitude(), alpha, fastener_force_vector,force.duration));
                }
                ConnectionShearFastenerCapacity connection_capacity = new ConnectionShearFastenerCapacity(single_capacities, spacing);
                fastener_capacities.Add(connection_capacity);
                fastener_forces.Add(forces);
            }

        }

        public ShearConnectionUtilization Util()
        {
            ShearConnectionUtilization utilizations = new ShearConnectionUtilization();
            for (int i = 0; i < fastener_forces.Count; i++)
            {
                List<double> load_case_utilization = new List<double>();
                List<string> load_case_failure_mode = new List<string>();
                List<FastenerForce> forces = fastener_forces[i];
                ConnectionShearFastenerCapacity capacities = fastener_capacities[i];
                for (int j = 0; j < forces.Count; j++)
                {
                    FastenerForce force = forces[j];
                    Dictionary<string,double> capacity = capacities.ShearResistance()[j];
                    double min_capacity = capacity.Values.Min();
                    string min_failure_mode = capacity.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                    double kmod = Utils.KMOD(service_class, force.duration);
                    double fastener_utilization = force.f / (kmod*min_capacity/1.3);
                    load_case_utilization.Add(fastener_utilization);
                    load_case_failure_mode.Add(min_failure_mode);
                }
                utilizations.AddUtilizations(load_case_utilization, load_case_failure_mode);
            }

            return utilizations;
        }

    }

    public class ShearConnectionUtilization
    {
        List<List<double>> utilization = new List<List<double>>();
        List<List<string>> failure_mode = new List<List<string>>();

        public ShearConnectionUtilization()
        {

        }

        public void AddUtilizations(List<double> utilizations, List<string> failure_modes)
        {
            utilization.Add(utilizations);
            failure_mode.Add(failure_modes);
        }
    }
}
