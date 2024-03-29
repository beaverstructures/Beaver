﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BeaverCore.Frame;
using BeaverCore.Geometry;
using BeaverCore.Actions;

namespace BeaverCore.Connections
{
    [Serializable]
    public class Spacing
    {
        public ShearSpacing shear_spacing;
        public AxialSpacing axial_spacing;

        public Spacing() { }

        public Spacing(ShearSpacing shear, AxialSpacing axial)
        {
            shear_spacing = shear;
            axial_spacing = axial;
        }
    }

    [Serializable]
    public class ShearSpacing
    {
        public double a1;       /// required spacing parallel to the grain
        public double a2;       /// required spacing perpendicular to the grain
        public double a3t;      /// required loaded end distance in tension
        public double a3c;      /// required loaded end distance in compression
        public double a4t;      /// required loaded edge distance in tension
        public double a4c;      /// required unloaded edge distance

        public double a1v;      /// actual spacing parallel to the grain
        public double a1h;      /// actual spacing perpendicular to the grain
        public double a3;       /// actual loaded end distance
        public double a4;       /// actual loaded edge distance

        public int npar;        /// number of screws parallel to grain
        public int nperp;       /// number of screws perpendicular to grain
        public bool staggered;  /// boolean indicating whether screws are staggered or not. Currently not considered.

        public Dictionary<int, Point2D> fastPositions; /// fastener positions in meters

        public ShearSpacing() { }

        /// <summary>
        /// Creates a generic ShearSpacing object based on spacing parameters.
        /// </summary>
        public ShearSpacing(double a1, double a2, double a3t, double a3c, double a4t, double a4c, int npar, int nperp)
        {
            this.a1 = a1;
            this.a2 = a2;
            this.a3t = a3t;
            this.a3c = a3c;
            this.a4t = a4t;
            this.a4c = a4c;
            this.npar = npar;
            this.nperp = nperp;
        }

        public ShearSpacing(double a1h, double a1v, double a3, double a4, int npar, int nperp)
        {
            this.a1h = a1h;
            this.a1v = a1v;
            this.a3 = a3;
            this.a4 = a4;
            this.npar = npar;
            this.nperp = nperp;
        }

        /// <summary>
        /// Construct the minimum requirements ShearSpacing object
        /// for provided fastener, load orientation and timer density.
        /// This is used later to define the acceptance of the spacing of
        /// the current connection.
        /// </summary>
        /// 


        public void SetSpacingLimits(Fastener fastener, double pk, double alfa, bool preDrilled)
        {
            if (fastener.type == "nail" || (fastener.type == "screw" && fastener.ds <= 6))
            {
                this.CalculateForNails(pk, fastener.ds, alfa, preDrilled);
            }
            else if (fastener.type == "bolt" || (fastener.type == "screw" && fastener.ds > 6))
            {
                this.CalculateForBolt(alfa, fastener.ds);
            }
            else if (fastener.type == "dowel")
            {
                this.CalculateForDowel(alfa, fastener.ds);
            }
        }


        void CalculateForNails(double pk, double ds, double alfa, bool preDrilled)
        {
            double inRad = alfa;
            double cosAlfa = Math.Abs(Math.Cos(inRad));
            double sinAlfa = Math.Abs(Math.Sin(inRad));

            if (preDrilled == false)
            {
                if (pk <= 420 && ds <= 6)
                {
                    if (ds < 5)
                    {
                        this.a1 = (5 + 5 * Math.Abs(cosAlfa)) * ds;
                        this.a4t = (5 + 2 * sinAlfa) * ds;
                    }
                    else if (ds >= 5)
                    {
                        this.a1 = (5 + 7 * Math.Abs(cosAlfa)) * ds;
                        this.a4t = (5 + 5 * sinAlfa) * ds;
                    }
                    this.a2 = 5 * ds;
                    this.a3t = (10 + 5 * cosAlfa) * ds;
                    this.a3c = 10 * ds;
                    this.a4c = 5 * ds;
                }
                else if (420 < pk && pk <= 500 && ds <= 6)
                {
                    if (ds < 0.005) this.a4t = (0.007 + 0.002 * sinAlfa) * ds;
                    else if (ds >= 5) this.a4t = (7 + 5 * sinAlfa) * ds;
                    this.a1 = (7 + 8 * Math.Abs(cosAlfa)) * ds;
                    this.a2 = 7 * ds;
                    this.a3t = (15 + 5 * cosAlfa) * ds;
                    this.a3c = 15 * ds;
                    this.a4c = 7 * ds;
                }
                else if (pk > 500 || ds > 6)
                {
                    this.a1 = (4 + 1 * Math.Abs(cosAlfa)) * ds;
                    this.a2 = (3 + Math.Abs(sinAlfa)) * ds;
                    this.a3t = (7 + 5 * cosAlfa) * ds;
                    this.a3c = 7 * ds;
                    if (ds < 5) this.a4t = (3 + 2 * sinAlfa) * ds;
                    else if (ds >= 5) this.a4t = (3 + 4 * sinAlfa) * ds;
                    this.a4c = 3 * ds;
                }

            }
            else if (preDrilled == true)
            {
                if (ds < 5) this.a4t = (3 + 2 * sinAlfa) * ds;
                else if (ds >= 5) this.a4t = (3 + 4 * sinAlfa) * ds;
                this.a1 = (4 + Math.Abs(cosAlfa)) * ds;
                this.a2 = (3 + Math.Abs(sinAlfa)) * ds;
                this.a3t = (7 + 5 * Math.Abs(cosAlfa)) * ds;
                this.a3c = 7 * ds;
                this.a4c = 3 * ds;
            }

        }

        void CalculateForBolt(double alfa, double ds)
        {
            double inRad = alfa * Math.PI / 180;
            double cosAlfa = Math.Cos(inRad);
            double sinAlfa = Math.Sin(inRad);
            this.a1 = (4 + Math.Abs(cosAlfa)) * ds;
            this.a2 = 4 * ds;
            this.a3t = Math.Max(7 * ds, 80);
            this.a3c = Math.Max((1 + 6 * sinAlfa) * ds, 4 * ds);
            this.a4t = Math.Max((2 + 2 * sinAlfa) * ds, 3 * ds);
            this.a4c = 3 * ds;
        }

        void CalculateForDowel(double alfa, double ds)
        {
            double inRad = alfa * Math.PI / 180;
            double cosAlfa = Math.Cos(inRad);
            double sinAlfa = Math.Sin(inRad);
            this.a1 = (3 + 2 * Math.Abs(cosAlfa)) * ds;
            this.a2 = 2 * ds;
            this.a3t = Math.Max(7 * ds, 80);
            this.a3c = Math.Max((this.a3t * Math.Abs(sinAlfa)) * ds, 3 * ds);
            this.a4t = Math.Max((2 + 2 * sinAlfa) * ds, 3 * ds);
            this.a4c = 3 * ds;
        }


    }

    [Serializable]
    public class AxialSpacing
    {

        public double a1;
        public double a2;
        public double a1CG;
        public double a2CG;

        double across;
        double e;

        public int npar;
        public int nperp;
        public int n;

        /// <summary>
        /// Creates a generic AxialSpacing object based on spacing parameters.
        /// </summary>
        /// 

        /// $$$ across & e were added in accordance to ETA-110024 (Eurotech) & ETA-110030 (Rothoblaas)


        public AxialSpacing(double a1, double a2, double a1CG, double a2CG, double across, double e, int npar, int nperp)
        {
            this.a1 = a1;
            this.a2 = a2;
            this.a1CG = a1CG;
            this.a1CG = a1CG;
            this.across = across;
            this.e = e;
            this.npar = npar;
            this.nperp = nperp;
            n = npar * nperp;
        }

        //EC5 Section 8.7.2
        /// <summary>
        /// Construct the minimum requirements AxialSpacing object
        /// for provided fastener.
        /// This is used later to define the acceptance of the spacing of
        /// the current connection.
        /// </summary>

        public AxialSpacing(Fastener fastener, double d)
        {
            if (fastener.type != "screw")
            {
                throw new ArgumentException("EC5 Section 8.7.2 - Fastener must be a screw for this evaluation");
            }
            else
            {
                this.a1 = 7 * d;
                this.a2 = 5 * d;
                this.across = 1.5 * d;
                this.e = 3.5 * d;
                this.a1CG = 10 * d;
                this.a2CG = 4 * d;
            }
        }

    }
}

