﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeaverCore.Materials;
using BeaverCore.Misc;

namespace BeaverCore.CrossSection
{
    [Serializable]
    public abstract class CroSec
    {

        public double A;
        public double Iy;
        public double Iz;
        public double It;
        public double Wy;
        public double Wz;
        public double ry;
        public double rz;
        public Material material;

        public abstract double GetsigMcrit(double lef,double E05,double G05);

    }

    

    
}
