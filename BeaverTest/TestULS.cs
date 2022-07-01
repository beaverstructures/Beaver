using System;
using NUnit.Framework;
using System.Collections.Generic;
using BeaverCore.Frame;
using BeaverCore.Actions;
using BeaverCore.CrossSection;
using BeaverCore.Materials;
using BeaverCore.Geometry;

namespace BeaverTest
{
    /// <summary>
    /// WIP. Tests example ULS calculation using TimberFramePoint 
    /// To Do:  - displacement? 
    ///         - use mat from spreadsheet rather than manual entry 
    ///         - verifying answers in relations to load combinations? 
    /// </summary>
    public class TestULS
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestULSDesign()
        {
            Displacement disp = new Displacement(1, "QA");
            List<Displacement> disps = new List<Displacement>() { disp };
            Force force = new Force(28, 3, 2, 0.2, 1.5, 0.4, "QA");
            List<Force> forces = new List<Force>() { force };

           
            Material mat = new Material("Glulam c","Softwood", 24*1e6, 17*1e6,
                0.5, 21.5, 2.5, 3.5, 11000,
                9100, 300, 650, 540,365,0); // kN/cm^2, EN 338:2016
            CroSec crosec = new CroSec_Rect(15, 15,mat);
            Point3D point = new Point3D();
            TimberFramePoint element = new TimberFramePoint(point,forces, disps, crosec, 1, 205, 205, 205, 0.7);
            TimberFrameULSResult result = element.ULSUtilization();

            // 

            /*
            Resultados

            Torsion - EC5, 6.1.8 
            UF 0.12

            Shear - EC5, 6.1.7 
            UF 0.1
            UF 0.14

            Combined Shear and Torsion
            UF 0.36

            Axial Tension and Compression (parallel to the grain) - EC5, 6.1.2 and 6.1.4 
            UF 0.2

            Bending - EC5, 6.1.6 
            σm,y,d UF 0.24 
            σm,z,d UF 0.06

            Combined Bending and Axial Tension or Compression - EC5, 6.2.3 and 6.2.4
            EC5, Eq. 6.17 
            UF 0.49
            EC5, Eq. 6.18
            UF 0.43

            Combined Bending and Axial Compression
            EC5, Eq. 6.19 UF 0.29
            EC5, Eq. 6.20 UF 0.23

            Lateral-Torsional Buckling about main axis
            UF 0.24

            Stability - Combined Bending and Axial Compression - EC5, 6.3.2 and 6.3.3
            0.29	EC5, Eq. 6.23
            0.23	EC5, Eq. 6.24
            0.06	EC5, Eq. 6.35
             */




            /// extract results 
            List<double[]> UtilsY = result.UtilsY;

            for (int i=0; i < UtilsY.Count; i++)
            {
                // Utils0 values for all load combinations 
                Console.WriteLine(UtilsY[i][0]);
            }
        }
    }
}

