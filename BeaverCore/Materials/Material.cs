using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

        public Material(string _name, string _type, double _fmk, double _ft0k, double _ft90k, double _fc0k, double _fc90k, double _fvk, double _E0mean, double _E05, double _E90mean, double _Gmean,double G05, double _ym=0)
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
            Ym = _ym == 0 ? _ym : GetYm(type);
            Bc = GetBc(type);
        }
        public Material(string _type)
        {
            string text = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();
            var reader = new StreamReader(File.OpenRead(text + "\\Beaver\\MATERIALPROP.csv"));
            bool stop = false;
            while (!reader.EndOfStream || stop == false)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (values[0] == _type)
                {
                    fc0k = Double.Parse(values[1]);
                    ft0k = Double.Parse(values[2]);
                    fmk = Double.Parse(values[3]);
                    fc90k = Double.Parse(values[4]);
                    ft90k = Double.Parse(values[5]);
                    fvk = Double.Parse(values[6]);
                    pk = 100 * Double.Parse(values[7]);
                    E0mean = Double.Parse(values[8]);
                    E05 = Double.Parse(values[9]);
                    G05 = Double.Parse(values[10]);
                    Ym = Double.Parse(values[12]);
                    name = values[13];
                    Bc = GetBc(type);

                    stop = true;
                }
            }
        }

        public static List<string> GetTypesNames()
        {

            List<string> names = new List<string>();
            string text = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();
            var reader = new StreamReader(File.OpenRead(text + "\\Beaver\\MATERIALPROP.csv"));
            bool cont = false;
            bool stop = false;
            var line = reader.ReadLine();
            while (!reader.EndOfStream || stop == false)
            {
                if (cont == false) { cont = true; }
                else {
                    var values = line.Split(',');
                    names.Add(values[0]); }
                line = reader.ReadLine();
                if (line == "END") { stop = true; }

            }
            return names;
        }

        public void Setkdef(int SC)
        {
            // Eurocode 5 Table 3.2
            switch (type)
            {
                case "Solid Timber":
                    switch (SC) {
                        case 1: kdef = 0.60; break;
                        case 2: kdef = 0.80; break;
                        case 3: kdef = 2.00; break;
                    }
                    break;
                case "Glulam":
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
                    return 0.2;
                case "Glulam":
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
                    return 1.3;
                case "Glulam":
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
