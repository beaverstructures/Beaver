﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Beaver_v0._1
{
    class Material
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
        public double E005;

        public double Gmean;
        public double G05;

        public double pk;
        public double Ym;

        public Material() { }

        public Material(string _name, string _type, double _fmk, double _ft0k, double _ft90k, double _fc0k, double _fc90k, double _fvk, double _E0mean, double _E005, double _Gmean, double _G05)
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
            E005 = _E005;
            Gmean = _Gmean;
            G05 = _G05;
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
                    E005 = Double.Parse(values[9]);
                    G05 = Double.Parse(values[10]);
                    Ym = Double.Parse(values[12]);
                    name = values[13];

                    stop = true;
                }
            }
        }

        public List<string> GetTypesNames()
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

    }
}