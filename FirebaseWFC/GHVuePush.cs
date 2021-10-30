using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FireSharp.Interfaces;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FirebaseWFC
{
public class GHVuePush : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHVuePush()
            : base("Push Vue", "Push Vue",
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
            pManager.AddPointParameter("CenterPoints", "CenterPoints", "Data", GH_ParamAccess.list);
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
            var data = new List<Point3d>();
            
            if (!DA.GetData(0, ref push)) return;
            if (!DA.GetDataList(1, data)) return;

            var config = Environment.GetEnvironmentVariable("firebaseConfig");

            _client = Firebase.GetOrCreateClient(_client, config);
            
            if (_client != null && push)
            {
                try
                {
                    var points = ParsePointsToVue(data);
                    var firebaseResponse = await _client.SetAsync("cubes", points);
                    DA.SetData(0, firebaseResponse.Body);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }

            }
        }

        private static List<Dictionary<string, Dictionary<string, double>>> ParsePointsToVue(List<Point3d> points)
        {
            return points.Select(point => new Dictionary<string, Dictionary<string, double>>
            {
                {
                    GenerateRandomString(), new Dictionary<string, double>
                    {
                        {"x", point.X},
                        {"y", point.Z},
                        {"z", point.Y},
                    }
                }
            }).ToList();
        }

        public static string GenerateRandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
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
        public override Guid ComponentGuid => new Guid("4aad354d-7498-461d-a90f-826d40a34d37");

        private IFirebaseClient _client = null;
    }
}