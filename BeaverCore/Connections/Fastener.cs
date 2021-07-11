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
        public double ds;   // Shank diameter
        public double dh;   // Head Diameter
        public double l;    
        public double lpen; // Penetration length of fastener
        public double tpen; // Penetration length of threaded part
        public double fu;   // Ultimate Strength of fastener
        public double t;    // Thickness of the headside member
        public string type;
        public bool smooth;
        public double alpha; // angle between fastener and timber grain

        // Fastener properties according to EN 14592
        public double faxk; 
        public double fheadk;
        public double rhoa;
        public double rhok; // Characteristic density in kg/m³

        public Fastener() { }

        public Fastener(string fastenerType = "", double D=0 , double Ds = 0, double Dh = 0, double L = 0, double Fu = 0, bool Smooth = true, double faxk=0,double fheadk=0)
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
                    break;
                case "Nail":
                    d = D;
                    ds = D;
                    dh = Dh;
                    l = L;
                    fu = Fu;
                    type = fastenerType;
                    smooth = Smooth;
                    this.faxk = faxk;
                    this.fheadk = fheadk;
                    break;
                default:
                    throw new ArgumentException("Fasterner type not found");
            }
    }

    public class FastenerForce
    {
        public double f = 0;
        public Vector2D force_vector;
        public double alpha = 0;
        public string duration;

        public FastenerForce(double f, double alpha,string duration)
        {
            this.f = f;
            this.alpha = alpha;
            this.duration = duration;
        }

        public FastenerForce(double f, double alpha,Vector2D force_vector,string duration)
        {
            this.f = f;
            this.alpha = alpha;
            this.force_vector = force_vector;
            this.duration = duration;
        }
    }
}
