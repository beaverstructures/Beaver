using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Beaver_v0._1
{
    public class BeaverInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Beaver";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Beaver is a collection of parametric tools for the design and analysis of timber structures according to the " +
                    "Eurocode 5 - Design of Timber Structures.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("90e18c2b-b29b-4aa9-af96-90f5dbb303b7");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Márcio Sartorelli, João Pini and Renan Prandini";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "beaver.structures@gmail.com";
            }
        }

        public override string AssemblyVersion
        {
            get
            {
                return "1.0.2";
            }
        }
    }
}
