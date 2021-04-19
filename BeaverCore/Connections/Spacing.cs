using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaverCore.Connections
{
    public class ShearSpacing
    {
        public double a1;
        public double a2;
        public double a3t;
        public double a3c;
        public double a4t;
        public double a4c;
        public int npar;
        public int npep;

        /// <summary>
        /// Creates a generic ShearSpacing object based on spacing parameters.
        /// </summary>
        public ShearSpacing(double a1, double a2, double a3t, double a3c, double a4t, double a4c, int npar, int npep)
        {
            this.a1 = a1;
            this.a2 = a2;
            this.a3t = a3t;
            this.a3c = a3c;
            this.a4t = a4t;
            this.a4c = a4c;
            this.npar = npar;
            this.npep = npep;
        }

        /// <summary>
        /// Construct the minimum requirements ShearSpacing object
        /// for provided fastener, load orientation and timer density.
        /// This is used later to define the acceptance of the spacing of
        /// the current connection.
        /// </summary>
        public ShearSpacing(Fastener fastener, double pk, double alfa, bool preDrilled)
        {
            if (fastener.type == "nail" || (fastener.type == "screw" && fastener.d <= 6))
            {
                this.CalculateForNails(pk, fastener.d, alfa);
            }
            else if (fastener.type == "bolt" || (fastener.type == "screw" && fastener.d > 6))
            {
                this.CalculateForBolt(alfa, fastener.d);
            }
            else if (fastener.type == "dowel")
            {
                this.CalculateForDowel(alfa, fastener.d);

            }
        }



        void CalculateForNails(double pk, double d, double alfa)
        {
            double inRad = alfa;
            double cosAlfa = Math.Cos(inRad);
            double sinAlfa = Math.Sin(inRad);
            if (pk <= 420 && d <= 6)
            {
                if (d < 5)
                {
                    this.a1 = (5 + 5 * Math.Abs(cosAlfa)) * d; this.a4t = (5 + 2 * sinAlfa) * d;
                }
                else if (d >= 5)
                {
                    this.a1 = (5 + 7 * Math.Abs(cosAlfa)) * d; this.a4t = (5 + 5 * sinAlfa) * d;
                }
                this.a2 = 5 * d;
                this.a3t = (10 + 5 * cosAlfa) * d;
                this.a3c = 10 * d;
                this.a4c = 5 * d;
            }
            else if (420 < pk && pk <= 500 && d <= 6)
            {
                if (d < 5) this.a4t = (7 + 2 * sinAlfa) * d;
                else if (d >= 5) this.a4t = (7 + 5 * sinAlfa) * d;
                this.a1 = (7 + 8 * Math.Abs(cosAlfa)) * d;
                this.a2 = 7 * d;
                this.a3t = (15 + 5 * cosAlfa) * d;
                this.a3c = 15 * d;
                this.a4c = 7 * d;
            }
            else if (pk > 500 || d > 6)
            {
                this.a1 = (4 + 1 * Math.Abs(cosAlfa)) * d;
                this.a2 = (3 + Math.Abs(sinAlfa)) * d;
                this.a3t = (7 + 5 * cosAlfa) * d;
                this.a3c = 7 * d;
                if (d < 5) this.a4t = (3 + 2 * sinAlfa) * d;
                else if (d >= 5) this.a4t = (3 + 4 * sinAlfa) * d;
                this.a4c = 3 * d;
            }
        }

        void CalculateForBolt(double alfa, double d)
        {
            double inRad = alfa * Math.PI / 180;
            double cosAlfa = Math.Cos(inRad);
            double sinAlfa = Math.Sin(inRad);
            this.a1 = (4 + Math.Abs(cosAlfa)) * d;
            this.a2 = 4 * d;
            this.a3t = Math.Max(7 * d, 80);
            this.a3c = Math.Max((1 + 6 * sinAlfa) * d, 4 * d);
            this.a4t = Math.Max((2 + 2 * sinAlfa) * d, 3 * d);
            this.a4c = 3 * d;
        }

        void CalculateForDowel(double alfa, double d)
        {
            double inRad = alfa * Math.PI / 180;
            double cosAlfa = Math.Cos(inRad);
            double sinAlfa = Math.Sin(inRad);
            this.a1 = (3 + 2 * Math.Abs(cosAlfa)) * d;
            this.a2 = 2 * d;
            this.a3t = Math.Max(7 * d, 80);
            this.a3c = Math.Max((this.a3t * Math.Abs(sinAlfa)) * d, 3 * d);
            this.a4t = Math.Max((2 + 2 * sinAlfa) * d, 3 * d);
            this.a4c = 3 * d;
        }




    }

    public class AxialSpacing
    {

        public double a1;
        public double a2;
        public double a1CG;
        public double a2CG;
        public int npar;
        public int npep;

        /// <summary>
        /// Creates a generic AxialSpacing object based on spacing parameters.
        /// </summary>
        public AxialSpacing(double a1, double a2, double a1CG, double a2CG, int npar, int npep)
        {
            this.a1 = a1;
            this.a2 = a2;
            this.a1CG = a1CG;
            this.a1CG = a1CG;
            this.npar = npar;
            this.npep = npep;
        }

        //EC5 Section 8.7.2
        /// <summary>
        /// Construct the minimum requirements AxialSpacing object
        /// for provided fastenern.
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
                this.a1CG = 10 * d;
                this.a2CG = 4 * d;
            }
        }

    }
}

