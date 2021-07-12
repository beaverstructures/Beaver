using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Geometry
{
    [Serializable]
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
    [Serializable]
    public class Vector2D
    {
        public double x;
        public double y;

        public Vector2D() { }

        public Vector2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2D fromPoint(Point2D point)
        {
            Vector2D vector = new Vector2D(point.x, point.y);
            return vector;
        }

        public double DotProduct(Vector2D other_vector)
        {
            return x * other_vector.x + y * other_vector.y;
        }

        public double Magnitude()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double AngletoVector(Vector2D other_vector)
        {
            double dot = this.DotProduct(other_vector);
            double cos = Math.Abs(dot) / (this.Magnitude() * other_vector.Magnitude());
            return Math.Acos(cos);
        }

        public Vector2D Unit()
        {
            return this / Magnitude();
        }

        public Vector2D RotatedVector(double radians)
        {
            double sin = Math.Sin(radians);
            double cos = Math.Cos(radians);
            double rotated_x = cos * x - sin * y;
            double rotated_y = sin * x + cos * y;
            return new Vector2D(rotated_x, rotated_y);
        }

        public static Vector2D operator +(Vector2D v1, Vector2D v2)
        {
            Vector2D result = new Vector2D();
            result.x = v1.x + v2.x;
            result.y = v1.y + v2.y;
            return result;
        }

        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            Vector2D result = new Vector2D();
            result.x = v1.x - v2.x;
            result.y = v1.y - v2.y;
            return result;
        }

        public static Vector2D operator *(double s, Vector2D v)
        {
            Vector2D result = new Vector2D();
            result.x = s * v.x;
            result.y = s * v.y;
            return result;
        }

        public static Vector2D operator *(Vector2D v, double s)
        {
            Vector2D result = new Vector2D();
            result.x = s * v.x;
            result.y = s * v.y;
            return result;
        }

        public static Vector2D operator /(Vector2D v, double s)
        {
            Vector2D result = new Vector2D();
            result.x = v.x / s;
            result.y = v.y / s;
            return result;
        }
        public List<double> ToList()
        {
            return new List<double>() { x, y };
        }
    }
    [Serializable]
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
            result.x = s * p.x;
            result.y = s * p.y;
            result.z = s * p.z;
            return result;
        }
    }
    [Serializable]
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
