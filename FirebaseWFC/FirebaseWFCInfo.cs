using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace FirebaseWFC
{
    public class FirebaseWFCInfo : GH_AssemblyInfo
    {
        public override string Name => "FirebaseWFC";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("CC9E0F61-EF62-4306-876C-66D3645C4298");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}