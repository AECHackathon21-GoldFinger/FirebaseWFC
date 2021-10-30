using System;
using System.Collections.Generic;
using System.Drawing;
using FireSharp.Interfaces;
using Grasshopper.Kernel;

namespace FirebaseWFC
{
    public class GHPushScore : GH_Component
    {
        public GHPushScore()
            : base("Push Score", "Score",
                "Description",
                "FirebaseWFC", "Connect")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Push", "Push", "Firebase Push",
                GH_ParamAccess.item);
            pManager.AddTextParameter("User", "User", "Data", GH_ParamAccess.item);
            pManager.AddTextParameter("LCA", "LCA", "Data", GH_ParamAccess.item);
            pManager.AddTextParameter("Cost", "Cost", "Data", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override async void SolveInstance(IGH_DataAccess DA)
        {
            var push = false;
            var user = "";
            var cost = "";
            var lca = "";
            var path = "users";

            if (!DA.GetData(0, ref push)) return;
            if (!DA.GetData(1, ref user)) return;
            DA.GetData(2, ref cost);
            DA.GetData(3, ref lca);

            var config = Environment.GetEnvironmentVariable("firebaseConfig");

            _client = Firebase.GetOrCreateClient(_client, config);

            if (_client != null && push)
            {
                try
                {
                    var data = ParseData(lca, cost);
                    var firebaseResponse = await _client.SetAsync($"{path}/{user}", data);
                    DA.SetData(0, firebaseResponse.Body);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }
        }

        private static Dictionary<string, string> ParseData(string lca, string cost)
        {
            return new Dictionary<string, string>{ { "lca", lca }, { "cost", cost } };
        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c50e3118-424b-4084-8041-b6a9531f46c3");

        private IFirebaseClient _client = null;
    }
}