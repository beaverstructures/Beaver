using System;

namespace BeaverCore.Materials
{
    [Serializable]
    public class Material
    {
        public string name;
        public string type;

        public double fmk;
        public double ft0k;
        public double ft90k;
        public double fc0k;
        public double fc90k;
        public double fvk;

        public double E0mean;
        public double E05;
        public double E90mean;

        public double Gmean;
        public double G05;

        public double pk;
        public double Ym;

        public double kdef;
        public double Bc;

        public Material() { }

        public Material(string _name, string _type, double _fmk, double _ft0k, 
            double _ft90k, double _fc0k, double _fc90k, double _fvk, double _E0mean, 
            double _E05, double _E90mean, double _Gmean,double G05, double pk , double _ym=0)
        {
            name = _name;
            type = _type;
            fmk = _fmk;
            ft0k = _ft0k;
            ft90k = _ft90k;
            fc0k = _fc0k;
            fc90k = _fc90k;
            fvk = _fvk;
            E0mean = _E0mean;
            this.G05 = G05;
            E05 = _E05;
            E90mean = _E90mean;
            Gmean = _Gmean;
            this.pk = pk;
            Ym = (_ym == 0) ? GetYm(type): _ym;
            Bc = GetBc(type);
        }

        public void defaultMaterial()
        {
            name = "GL24c";
            type = "Glulam c";
            fmk = 24;
            ft0k = 17;
            ft90k = 0.5;
            fc0k = 21.5;
            fc90k = 2.5;
            fvk = 3.5;
            E0mean = 11000;
            G05 = 250;
            E05 = 9100;
            E90mean = 300;
            Gmean = 650;
            pk = 365;
            Ym = 1.3;
            Bc = 0.2;
        }

        public void Setkdef(int SC)
        {
            // Eurocode 5 Table 3.2
            switch (type)
            {
                case "Solid Timber":
                case "Softwood":
                case "Hardwood":
                    switch (SC) {
                        case 1: kdef = 0.60; break;
                        case 2: kdef = 0.80; break;
                        case 3: kdef = 2.00; break;
                    }
                    break;
                case "Glulam":
                case "Gluelam c":
                case "Gluelam h":
                    switch (SC) {
                        case 1: kdef = 0.60; break;
                        case 2: kdef = 0.80; break;
                        case 3: kdef = 2.00; break;
                    }
                    break;
                case "LVL":
                    switch (SC)
                    {
                        case 1: kdef = 0.60; break;
                        case 2: kdef = 0.80; break;
                        case 3: kdef = 2.00; break;
                    }
                    break;
                default:
                    throw new ArgumentException("Timber material type not found");
            }
        }

        private double GetBc(string type)
        {
            // Eurocode 5, Section 6.3.2.
            switch (type)
            {
                case "Solid Timber":
                case "Softwood":
                case "Hardwood":
                    return 0.2;
                case "Glulam":
                case "Glulam c":
                case "Glulam h":
                    return 0.1;
                case "LVL":
                    return 0.1;
                default:
                    throw new ArgumentException("Timber material type not found");
            }
        }

        private double GetYm(string type)
        {
            // EC5, 2.4.1, Tab. 2.3.
            switch (type)
            {
                case "Solid Timber":
                case "Softwood":
                case "Hardwood":
                    return 1.3;
                case "Glulam":
                case "Glulam c":
                case "Glulam h":
                    return 1.25;
                case "LVL":
                    return 1.2;
                case "Plywood":
                    return 1.2;
                case "OSB":
                    return 1.2;
                default:
                    throw new ArgumentException("Timber material type not found");
            }
        }
    }
}
