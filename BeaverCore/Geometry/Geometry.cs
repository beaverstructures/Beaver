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
        public static Point2D operator -(Point2D p1, Point2D p2)
        {
            return new Point2D(p1.x - p2.x, p1.y - p2.y);
        }

        public List<double> ToList()
        {
            return new List<double>() { x, y };
        }

        public double Distance(Point2D pt)
        {
            return Math.Sqrt(Math.Pow(this.x - pt.x, 2) + Math.Pow(this.y - pt.y, 2));
        }

        public double deltaX(Point2D pt)
        {
            return this.x - pt.x;
        }
        public double deltaY(Point2D pt)
        {
            return this.y - pt.y;
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
            Point3D result = new Point3D
            {
                x = p1.x + p2.x,
                y = p1.y + p2.y,
                z = p1.z + p2.z
            };
            return result;
        }

        public static Point3D operator -(Point3D p1, Point3D p2)
        {
            Point3D result = new Point3D
            {
                x = p1.x - p2.x,
                y = p1.y - p2.y,
                z = p1.z - p2.z
            };
            return result;
        }

        public static Point3D operator *(double s, Point3D p)
        {
            Point3D result = new Point3D
            {
                x = s * p.x,
                y = s * p.y,
                z = s * p.z
            };
            return result;
        }
        public static double DistanceTo(Point3D startpt, Point3D endpt)
        {
            Point3D distance = endpt - startpt;
            double dist = Math.Sqrt(Math.Pow(distance.x, 2) + Math.Pow(distance.y, 2) + Math.Pow(distance.z, 2));
            return dist;
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
            Vector2D result = new Vector2D
            {
                x = v1.x + v2.x,
                y = v1.y + v2.y
            };
            return result;
        }

        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            Vector2D result = new Vector2D
            {
                x = v1.x - v2.x,
                y = v1.y - v2.y
            };
            return result;
        }

        public static Vector2D operator *(double s, Vector2D v)
        {
            Vector2D result = new Vector2D
            {
                x = s * v.x,
                y = s * v.y
            };
            return result;
        }

        public static Vector2D operator *(Vector2D v, double s)
        {
            Vector2D result = new Vector2D
            {
                x = s * v.x,
                y = s * v.y
            };
            return result;
        }

        public static Vector2D operator /(Vector2D v, double s)
        {
            Vector2D result = new Vector2D
            {
                x = v.x / s,
                y = v.y / s
            };
            return result;
        }
        public List<double> ToList()
        {
            return new List<double>() { x, y };
        }
    }

    [Serializable]
    public class Vector3D
    {
        public double x;
        public double y;
        public double z;

        public Vector3D() { }

        public Vector3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3D(Vector3D vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public static Vector3D fromPoint(Point3D point)
        {
            Vector3D vector = new Vector3D(point.x, point.y, point.z);
            return vector;
        }

        public double DotProduct(Vector3D vector)
        {
            return x * vector.x + y * vector.y + z * vector.z;
        }

        public Vector3D CrossProduct(Vector3D v1)
        {
            return new Vector3D()
            {
                x = this.y * v1.z - this.z * v1.y,
                y = -(this.y * v1.y - this.z * v1.x),
                z = this.x * v1.y - this.y * v1.x
            };
            
        }

        public double Magnitude()
        {
            return Math.Sqrt(x * x + y * y + z*z);
        }

        public double AngletoVector(Vector3D other_vector)
        {
            return Math.Acos(DotProduct(other_vector) / (Magnitude() * other_vector.Magnitude()));
        }

        public Vector3D Unit()
        {
            return this / Magnitude();
        }

        public Vector3D RotatedVector(double radians)
        {
            throw new NotImplementedException();
        }

        public static Vector3D operator +(Vector3D v1, Vector3D v2)
        {
            Vector3D result = new Vector3D
            {
                x = v1.x + v2.x,
                y = v1.y + v2.y,
                z = v1.z + v2.z
            };
            return result;
        }

        public static Vector3D operator -(Vector3D v1, Vector3D v2)
        {
            Vector3D result = new Vector3D
            {
                x = v1.x - v2.x,
                y = v1.y - v2.y,
                z = v1.z - v2.z
            };
            return result;
        }

        public static Vector3D operator *(double s, Vector3D v)
        {
            Vector3D result = new Vector3D
            {
                x = s * v.x,
                y = s * v.y,
                z = s * v.z
            };
            return result;
        }

        public static Vector3D operator *(Vector3D v, double s)
        {
            Vector3D result = new Vector3D
            {
                x = s * v.x,
                y = s * v.y,
                z = s * v.z,
            };
            return result;
        }

        public static Vector3D operator /(Vector3D v, double s)
        {
            Vector3D result = new Vector3D
            {
                x = v.x / s,
                y = v.y / s,
                z = v.z / s,
            };
            return result;
        }
        public List<double> ToList()
        {
            return new List<double>() { x, y , z};
        }
    }

    public class Plane
    {
        public Point3D origin;
        public Vector3D U;
        public Vector3D V;
        
        public Plane(Point3D origin, Vector3D U, Vector3D V)
        {
            this.origin = origin;
            this.U = U;
            this.V = V;
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
            Point3D pt = start + relpos * (end - start);
            return pt;
        }
    }

    [Serializable]
    public class Polyline
    {

        public List<Point3D> pts;
        public double length=0;
        public double endptsLength;

        public Polyline() { }

        public Polyline(List<Point3D> pts)
        {
            this.pts = pts;
            SetLength();
            SetEndptsLength();
        }

        public void SetLength()
        {
            for(int i = 1; i < pts.Count; i++)
            {
                length += Point3D.DistanceTo(pts[i - 1], pts[i]);
            }
        }
        public void SetEndptsLength()
        {
            endptsLength = Point3D.DistanceTo(pts[0], pts[pts.Count-1]);
        }
        public bool IsValid()
        {
            if (pts.Count > 0) return true;
            else return false;
        }

        public static implicit operator Polyline(Line v)
        {
            List<Point3D> pts = new List<Point3D> { 
                v.start,
                v.end
            };
            return new Polyline(pts);
        }
    }
}
