using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BeaverCore.Geometry;
using BeaverCore.Materials;
using BeaverCore.Frame;
using BeaverCore.Actions;

using Rhino.Geometry;

using Karamba.Geometry;
using Karamba.CrossSections;

namespace BeaverGrasshopper.Components.Utilities
{
    public static class KarambaUtilities
    {
        public static Point3D K3Dpt2Beaver(this Point3 karamba_point)
        {
            return new Point3D(karamba_point.X, karamba_point.Y, karamba_point.Z);
        }

        public static BeaverCore.CrossSection.CroSec K3DToBeaver(this CroSec karamba_crosec)
        {
            if (karamba_crosec is CroSec_Trapezoid)
            {
                CroSec_Trapezoid trapezoid_crosec = (CroSec_Trapezoid)karamba_crosec;
                double width = Math.Min(trapezoid_crosec.lf_width, trapezoid_crosec.uf_width);
                BeaverCore.CrossSection.CroSec beaver_crosec = new BeaverCore.CrossSection.CroSec_Rect(
                    trapezoid_crosec._height, 
                    width, 
                    karamba_crosec.material.K3DToBeaver());
                return beaver_crosec;
            }
            else if (karamba_crosec is CroSec_Circle)
            {
                CroSec_Circle circle_crosec = (CroSec_Circle)karamba_crosec;
                BeaverCore.CrossSection.CroSec beaver_crosec = new BeaverCore.CrossSection.CroSec_Circ(circle_crosec.getHeight(), karamba_crosec.material.K3DToBeaver());
                return beaver_crosec;
            }
            else
            {
                throw new ArgumentException("Karamba to Beaver Conversion only supports Karamba Trapezoid and Circle Cross Sections");
            }
        }

        public static BeaverCore.Materials.Material K3DToBeaver(this Karamba.Materials.FemMaterial k3dMaterial)
        {
            if (k3dMaterial.HasUserData())
            {
                Material beaverMaterial = new Material
                {
                    type = k3dMaterial.family,
                    name = k3dMaterial.name,
                    fmk = (double)k3dMaterial.UserData["fmk"] * 1e3, // kN/m² to Pa
                    ft0k = k3dMaterial.ft(0) * 1e3,
                    ft90k = k3dMaterial.ft(1) * 1e3,
                    fc0k = k3dMaterial.fc(0) * 1e3,
                    fc90k = k3dMaterial.fc(1) * 1e3,

                    fvk = (double)k3dMaterial.UserData["fvk"] * 1e3,
                    frk = (double)k3dMaterial.UserData["frk"] * 1e3,

                    E0mean = k3dMaterial.E(0) * 1e3,
                    E05 = (double)k3dMaterial.UserData["E05"] * 1e3,
                    E90mean = k3dMaterial.E(1) * 1e3,
                    E90_05 = (double)k3dMaterial.UserData["E90_05"] * 1e3,
                    Gmean = k3dMaterial.G12() * 1e3,
                    G05 = k3dMaterial.G3() * 1e3,

                    pk = k3dMaterial.gamma(),

                    Ym = (double)k3dMaterial.UserData["ym"],
                    kdef = (double)k3dMaterial.UserData["kdef"],
                    Bc = (double)k3dMaterial.UserData["Bc"],
                    pmean = (double)k3dMaterial.UserData["pmean"] * 1000 // kN/m³ to N/m³
                };
                beaverMaterial.pk = (double)k3dMaterial.gamma() * 1000;
                return beaverMaterial;
            }
            else
            {
                throw new Exception("Beaver parameters not set in the Karamba Material. Use the BeaverToKarambaMaterial component.");
            }
        }

        public static BeaverCore.Frame.TimberFrame.SpanLine ImportSpanLineProperties(
            BeaverCore.Geometry.Polyline poly, 
            Karamba.Models.Model k3dModel, 
            List<string> lc_types)
        {
            // finds node indexes of start and end of the polyline, 
            // retrieves the nodal displacements and 
            // returns a SpanLine object with properties assigned

            Point3 k3dpoint1 = new Point3(
                poly.pts[0].x,
                poly.pts[0].y,
                poly.pts[0].z);
            Point3 k3dpoint2 = new Point3(
                poly.pts[poly.pts.Count - 1].x,
                poly.pts[poly.pts.Count - 1].y,
                poly.pts[poly.pts.Count - 1].z);
            List<int> nodeIDs = new List<int>(){
                k3dModel.NodeInd(k3dpoint1, 0.01),
                k3dModel.NodeInd(k3dpoint2, 0.01) };

            // List-structure: load-case/node.
            List<List<Vector3>> vectorsTranslation = new List<List<Vector3>>();
            List<List<Vector3>> vectorsRotation = new List<List<Vector3>>();
            TimberFrame.SpanLine spanLine = new TimberFrame.SpanLine(poly);

            Karamba.Results.NodalDisp.solve(k3dModel, null, nodeIDs, out vectorsTranslation, out vectorsRotation);
            for (int i = 0; i < lc_types.Count; i++)
            {
                spanLine.startDisp.Add(new Displacement(
                    vectorsTranslation[i][0].X,
                    vectorsTranslation[i][0].Y,
                    vectorsTranslation[i][0].Z,
                    lc_types[i]));
                spanLine.endDisp.Add(new Displacement(
                    vectorsTranslation[i][1].X,
                    vectorsTranslation[i][1].Y,
                    vectorsTranslation[i][1].Z,
                    lc_types[i]));
                spanLine.midDisp.Add(
                    (spanLine.startDisp[i] + spanLine.endDisp[i]) * 0.5);
            }
            return spanLine;
        }

        public static List<T> CreateList<T>(int capacity)
        {
            return Enumerable.Repeat(default(T), capacity).ToList();
        }

    }
}
