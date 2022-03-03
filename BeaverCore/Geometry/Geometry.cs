﻿using System;
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
            double dist = Math.Sqrt(Math.Pow(distance.x,2) + Math.Pow(distance.y, 2)+ Math.Pow(distance.z, 2));
            return dist;
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
