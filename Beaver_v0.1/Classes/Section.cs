using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaver_v0._1.Classes
{
    public class Section
    {
        
        public CroSec CS;
        public Material Mat;


        public Section() { }

        public Section(double n, double vy, double vz, double mx, double my, double mz, CroSec cs, Material m)
        {
            CS = cs;
            Mat = m;
        }

        public double[] BendingNormalUtil()
        {
            double[] result = new double[2];


            return result;
        }

        public double[] ShearUtil()
        {
            double[] result = new double[2];


            return result;
        }

        public double TorsionUtil()
        {
            double result = 0;


            return result;
        }
    }
}
