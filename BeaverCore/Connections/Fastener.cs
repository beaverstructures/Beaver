using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeaverCore.Geometry;

namespace BeaverCore.Connections
{
    [Serializable]
    public class Fastener
    {
        public double d;
        public double ds;       // Shank diameter
        public double dh;       // Head Diameter
        public double l;    
        public double lpen;     // Penetration length of fastener
        public double lth;      // Threaded Length
        public double offset;   // Offset from timber face
        public double tpen;     // Penetration length of threaded part
        public double fu;       // Ultimate Strength of fastener
        public double t;        // Thickness of the headside member
        public string type;
        public bool smooth;
        public double alpha;

        // Recently added properties
        public double Ymsteel;
        public Vector2D vector;

        public bool predrilled1;
        public bool predrilled2;

        public double t1; 
        public double t2;
        public double ts;

        public double b1;       // Thickness of timber element 1
        public double b2;       // Thickness of timber element 2


        // Fastener properties according to EN 14592
        public double faxk;     // characteristic pointside withdrawal strength
        public double fheadk;   // Characteristic headside pull-through strength
        public double rhoa;     // 
        public double rhok;     // Characteristic timber density in kg/m³

        public Fastener() { }

        public Fastener(string fastenerType, double D=0 , double Ds = 0, double Dh = 0,
            double L = 0, double Fu = 4000000000, bool Smooth = true, double faxk= 4.5e-6,
            double fheadk=0, double offset =0 , double lth=0,double b1=0,double b2=0, double Ymsteel = 1.05)
        {
            switch (fastenerType)
            {
                case "Dowel":
                    d = D;
                    ds = D;
                    dh = D;
                    l = L;
                    fu = Fu;
                    type = fastenerType;
                    smooth = true;
                    this.faxk = faxk;
                    this.fheadk = fheadk;
                    this.Ymsteel = Ymsteel;
                    break;
                case "Screw":
                    d = D;
                    ds = Ds;
                    dh = Dh;
                    l = L;
                    fu = Fu;
                    type = fastenerType;
                    smooth = true;
                    this.faxk = faxk;
                    this.fheadk = fheadk;
                    this.Ymsteel = Ymsteel;
                    break;
                case "Bolt":
                    d = D;
                    ds = D;
                    dh = Dh;
                    l = L;
                    fu = Fu;
                    type = fastenerType;
                    smooth = true;
                    this.faxk = faxk;
                    this.fheadk = fheadk;
                    this.Ymsteel = Ymsteel;
                    break;
                case "Nail":
                    d = D;
                    ds = D;
                    dh = Dh;
                    l = L;
                    fu = Fu;
                    type = fastenerType;
                    smooth = Smooth;
                    switch (tpen)
                    {
                        /// EC5 SECTION 8.3.2 (6) TO (7)
                        case double n when (n >= 12*d):
                            this.faxk = 20e-6 * Math.Pow(rhok, 2);
                            break;
                        case double n when (n < 12 * d && n >= 8 * d):
                            double reduction = smooth ? tpen/(4*d -2)  : tpen / (2 * d - 3);
                            this.faxk = 20e-6 * Math.Pow(rhok, 2)*reduction;
                            break;
                        case double n when (n < 8 * d):
                            this.faxk = 1e-10;
                            break;
                    }
                    this.fheadk = 20e-6 * Math.Pow(rhok, 2);
                    this.Ymsteel = Ymsteel;
                    break;
                default:
                    throw new ArgumentException("Fasterner type not found");
            }

            double sin = Math.Sin(Math.PI / 180 * alpha);
            double cos = Math.Cos(Math.PI / 180 * alpha);
            vector = new Vector2D(sin, cos);

            t1 = b1 - sin*offset;
            t2 = l - t1;
            lpen = Math.Min(t1, t2);
            double tpen2 = t2 > lth ? lth : t2;
            double tpen1 = t2 > lth ? 0 : lth - t2;

        }
    }
    public class FastenerForce
    {
        public double Faxd = 0;
        public double Fvd = 0;
        public Vector2D force_vector;
        public double alpha = 0;
        public string duration;

        public FastenerForce(double faxd=0, double fvd=0, double alpha=0, string duration ="")
        {
            Faxd = faxd;
            Fvd = fvd;
            this.alpha = alpha;
            this.duration = duration;
        }
    }

}
