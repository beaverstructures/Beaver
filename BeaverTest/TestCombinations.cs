using NUnit.Framework;
using System.Collections.Generic;
using BeaverCore.Actions;
using BeaverCore.Frame;
using BeaverCore.CrossSection;
using BeaverCore.Materials;
using System.Linq;

namespace BeaverTest
{
    public class TestCombinations
    {
        [SetUp]
        public void Setup()
        {
           
        }


        [Test]
        public void TestColumnCombinations()
        {
            // Results retrieved using RFEM for a column.
            Displacement disp1 = new Displacement(1.4, "P");
            Displacement disp2 = new Displacement(0.2, "QH");
            Displacement disp3 = new Displacement(-0.3, "QA");
            Displacement disp4 = new Displacement(14.5, "W1");
            Displacement disp5 = new Displacement(0.5, "W2");
            List<Displacement> disps = new List<Displacement>() { disp1, disp2, disp3, disp4, disp5 };
            Force force1 = new Force(-108.16, -1.14, 0, 0, 0, 0, "P");
            Force force2 = new Force(-4.05, -0.2, 0, 0, 0, 0, "QH");
            Force force3 = new Force(-22.73, 0.27, 0, 0, 0, 0, "QA");
            Force force4 = new Force(2.19, -1.92, -1.26, 0, 2.87, -4.38, "W1");
            Force force5 = new Force(11.56, 0.48, -0.06,0, 0.14, 1.09, "W2");
            List<Force> forces = new List<Force>() { force1, force2, force3, force4, force5 };
            CroSec crosec = new CroSec_Rect(15, 15);
            Material mat = new Material("ita", "Glulam", 5.5, 5, 0, 4.5, 0.2, 0.65, 1550, 1250, 106, 80);
            crosec.Mat = mat;
            TimberFramePoint timber = new TimberFramePoint(forces, disps, crosec, 2, 228.5, 228.5, 228.5, 0.9);
            List<string> combinations = new List<string>();
            foreach (Action a in timber.SLSComb.CreepDisplacements)
            {
                combinations.Add(a.combination);
            }
            Assert.AreEqual(14, timber.SLSComb.CharacteristicDisplacements.Count());
            Assert.AreEqual(14, timber.SLSComb.CreepDisplacements.Count());
            Assert.AreEqual(28, timber.ULSComb.DesignForces.Count());
        }
    }
}