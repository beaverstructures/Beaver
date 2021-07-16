using System;
using System.Collections.Generic;
using System.Text;
using BeaverCore.Materials;

namespace BeaverCore.Connections
{
    class SteelPlate
    {
        Material mat;       // steel material assigned
        
        double b;           // width in frame local axis X
        double t;           // thickness in frame local axis Y
        double h;           // height in frame local axis Z

        double offset_x;    // offset in frame local axis x 
        double offset_y;    // offset in frame local axis Y
        double offset_z;    // offset in frame local axis z

        double n;           // number of plates on the connection
    }
}
