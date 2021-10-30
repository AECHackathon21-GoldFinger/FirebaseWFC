using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FireSharp.Interfaces;
using FireSharp.Response;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FirebaseWFC
{
    public class GHFirebasePush : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHFirebasePush()
            : base("Push", "Push",
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
            pManager.AddPointParameter("Slots", "Slots", "Data", GH_ParamAccess.list);
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
                    var slots = ParsePointsToSlot(data);
                    var firebaseResponse = await _client.SetAsync("slots", slots);
                    DA.SetData(0, firebaseResponse.Body);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }

            }
        }

        private static List<int[]> ParsePointsToSlot(List<Point3d> points)
        {
            return points.Select(point => new int[] { (int)point.X, (int)point.Y, (int)point.Z }).ToList();
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
        public override Guid ComponentGuid => new Guid("c89fcdfb-6405-4427-bd67-a16f91df00ee");

        private IFirebaseClient _client = null;
    }
}