using System;
using NUnit.Framework;
using System.Collections.Generic;
using BeaverCore.Frame;
using BeaverCore.Actions;
using BeaverCore.CrossSection;
using BeaverCore.Materials;

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

            CroSec crosec = new CroSec_Rect(15, 15);
            Material mat = new Material("C18", "Solid", 1.8, 1, 0.04, 1.8, 0.22, 0.34, 900, 600, 56, 30, 1.3); // kN/cm^2, EN 338:2016
            crosec.material = mat;

            TimberFramePoint element = new TimberFramePoint(forces, disps, crosec, 1, 205, 205, 205, 0.7);
            TimberFrameULSResult result = element.ULSUtilization();

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

