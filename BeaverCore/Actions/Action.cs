using System;
using System.Collections.Generic;
using System.Text;

namespace BeaverCore.Actions
{
    [Serializable]
    public class Action
    {
        public string type;
        public string duration;
        public string combination;
        
        public TypeInfo typeinfo; // $$$ sugestão: trocar variavel type por typeinfo e remover duration

        //public Action(string info)
        //{
        //    typeinfo = new TypeInfo(info);
        //}

    }

    [Serializable]
    public class TypeInfo
    {
        public double phi0;
        public double phi1;
        public double phi2;
        public string duration;

        public TypeInfo() { }

        public TypeInfo(string type)
        {
            // Eurocode 0: EN 1990 table A1.1
            if (type.Contains("P")) { phi0 = 1; phi1 = 1; phi2 = 1; duration = "perm"; }            // permanent loads
            if (type.Contains("A")) { phi0 = 0.7; phi1 = 0.5; phi2 = 0.3; duration = "medium"; }    // domestic, residential
            if (type.Contains("B")) { phi0 = 0.7; phi1 = 0.5; phi2 = 0.3; duration = "medium"; }    // office
            if (type.Contains("C")) { phi0 = 0.7; phi1 = 0.7; phi2 = 0.6; duration = "medium"; }    // congregation areas
            if (type.Contains("D")) { phi0 = 0.7; phi1 = 0.7; phi2 = 0.6; duration = "medium"; }    // shopping areas
            if (type.Contains("E")) { phi0 = 1; phi1 = 0.9; phi2 = 0.8; duration = "long"; }        // storage areas
            if (type.Contains("F")) { phi0 = 0.7; phi1 = 0.7; phi2 = 0.6; duration = "short"; }     // traffic areas vehicle < 30kN
            if (type.Contains("G")) { phi0 = 0.7; phi1 = 0.5; phi2 = 0.3; duration = "short"; }     // traffic areas vehicle < 160kN
            if (type.Contains("H")) { phi0 = 0; phi1 = 0; phi2 = 0; duration = "short"; }           // roofs
            if (type.Contains("S")) { phi0 = 0.7; phi1 = 0.5; phi2 = 0.2; duration = "medium"; }    // Snow Load
            if (type.Contains("W")) { phi0 = 0.6; phi1 = 0.2; phi2 = 0; duration = "short"; }       // Wind load

            if (type.Contains("X")) { phi0 = 1; phi1 = 1; phi2 = 1; duration = "medium"; }          // Already calculated phis
        }
    }
}
