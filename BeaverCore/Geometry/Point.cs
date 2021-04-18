using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Geometry
{
    public class Point2D
    {
        public double x;
        public double y;

        public Point2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public List<double> ToList()
        {
            return new List<double>() { x, y };
        }
    }

    public class Point3D
    {
        public double x;
        public double y;
        public double z;

        public Point3D() { }

        public Point3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public List<double> ToList()
        {
            return new List<double>() { x, y, z };
        }

        public static Point3D operator +(Point3D p1, Point3D p2)
        {
            Point3D result = new Point3D();
            result.x = p1.x + p2.x;
            result.y = p1.y + p2.y;
            result.z = p1.z + p2.z;
            return result;
        }

        public static Point3D operator -(Point3D p1, Point3D p2)
        {
            Point3D result = new Point3D();
            result.x = p1.x - p2.x;
            result.y = p1.y - p2.y;
            result.z = p1.z - p2.z;
            return result;
        }

        public static Point3D operator *(double s, Point3D p)
        {
            Point3D result = new Point3D();
            result.x = s* p.x;
            result.y = s* p.y;
            result.z = s* p.z;
            return result;
        }
    }

    public class Line
    {
        public Point3D start;
        public Point3D end;

        public Line(Point3D start, Point3D end)
        {
            this.start = start;
            this.end = end;
        }

        public Point3D PointAtRelativePosition(double relpos)
        {
            return start + relpos * (end - start);
        }


    }
}
