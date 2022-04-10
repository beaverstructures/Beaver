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
        public bool countersunk;// EC3 Section 3.9

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

        public double Fax_Rd;
        public double Fv_Rd;

        public Fastener() { }

        public Fastener(string fastenerType, double D=0 , double Ds = 0, double Dh = 0,
            double L = 0, double Fu = 4000000000, bool Smooth = true, double faxk= 4.5e-6,
            double fheadk=0, double offset =0 , double lth=0,double b1=0,double b2=0, double Ymsteel = 1.05, bool countersunk = false)
        {
            d = D;
            l = L;
            fu = Fu;
            type = fastenerType;
            
            this.faxk = faxk;
            this.fheadk = fheadk;
            this.Ymsteel = Ymsteel;

            switch (fastenerType)
            {
                case "Dowel":
                    ds = D;
                    dh = D;
                    smooth = true;
                    break;
                case "Screw":
                    ds = Ds;
                    dh = Dh;
                    smooth = Smooth;
                    this.lth = lth;
                    break;
                case "Bolt":
                    ds = D;
                    dh = Dh;
                    smooth = true;
                    this.faxk = faxk;
                    this.countersunk = countersunk;
                    this.lth = lth;
                    break;
                case "Nail":
                    ds = D;
                    dh = Dh;
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
        public Vector3D Faxd = new Vector3D();
        public Vector3D Fvd = new Vector3D();
        public Vector3D Fx = new Vector3D();
        public Vector3D Fy = new Vector3D();
        public Vector3D Fz = new Vector3D();
        public Vector3D Fi_My = new Vector3D();
        public Vector3D Fi_Mz = new Vector3D();
        public Vector3D Fi_Mt = new Vector3D();
        public Vector3D force_vector;
        public double alpha = 0;
        public string duration;

        public FastenerForce(double faxd=0, double fvd=0, double alpha=0, string duration ="")
        {
            Faxd = new Vector3D().Unit() * (faxd);
            Fvd = new Vector3D().Unit() * (fvd);
            this.alpha = alpha;
            this.duration = duration;
        }
    }

}
