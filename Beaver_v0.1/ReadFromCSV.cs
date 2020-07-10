using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace Beaver_v0._1
{
    class ReadFromCSV
    {
        public double d;
        public double d_head;
        public double l;
        public double l_thread;
        public double fyk;
        public double pk;
        public string woodtype;
        //Vamos adicionando os parametros, voces que tao calculando devem saber melhor quais são, por hor ato fazendo só dos parafusos e madeira

        public ReadFromCSV() { }

        public void ReadScrewfromCSV(string screwname)
        {
            string text = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            text = Path.Combine(Directory.GetParent(text).FullName, "Plug-ins");
            var reader = new StreamReader(File.OpenRead(text + "\\Madeira\\Screws.csv"));
            int cont = -4;
            bool stop = false;
            while (!reader.EndOfStream || stop == false)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (values[1] == screwname)
                {
                    d = Double.Parse(values[2]);
                    l = Double.Parse(values[3]);
                    l_thread = Double.Parse(values[4]);
                    fyk = Double.Parse(values[9]);
                    stop = true;
                }
            }
        }

        public void ReadNailfromCSV(string nailname)
        {
            string text = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            text = Path.Combine(Directory.GetParent(text).FullName, "Plug-ins");
            var reader = new StreamReader(File.OpenRead(text + "\\Madeira\\Nails.csv"));
            int cont = -4;
            bool stop = false;
            while (!reader.EndOfStream || stop == false)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (values[1] == nailname)
                {
                    d = Double.Parse(values[2]);
                    l = Double.Parse(values[3]);
                    d_head = Double.Parse(values[4]);
                    fyk = Double.Parse(values[9]); //valores nao corretos
                    stop = true;
                }
            }
        }

        public void ReadBoltfromCSV(string boltname)
        {
            string text = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            text = Path.Combine(Directory.GetParent(text).FullName, "Plug-ins");
            var reader = new StreamReader(File.OpenRead(text + "\\Madeira\\Bolts.csv"));
            int cont = -4;
            bool stop = false;
            while (!reader.EndOfStream || stop == false)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (values[1] == boltname)
                {
                    d = Double.Parse(values[2]);
                    l = Double.Parse(values[3]);
                    d_head = Double.Parse(values[4]);
                    fyk = Double.Parse(values[9]); //valores nao corretos
                    stop = true;
                }
            }
        }

        public void ReadWoodfromCSV(string woodname)
        {
            string text = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            text = Path.Combine(Directory.GetParent(text).FullName, "Plug-ins");
            var reader = new StreamReader(File.OpenRead(text + "\\Madeira\\MLCPROP.csv"));
            int cont = -1;
            bool stop = false;
            while (!reader.EndOfStream || stop == false)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (values[0] == woodname)
                {
                    pk = 1000 * Double.Parse(values[7]);
                    woodtype = values[13];
                    stop = true;
                }
            }
        }
        

    }
}
