using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using FireSharp.Interfaces;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace FirebaseWFC
{
 public class GHVueSubscribe : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHVueSubscribe()
            : base("Get Vue", "Get Vue",
                "Description",
                "FirebaseWFC", "Connect")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Subscribe", "Subscribe", "Firebase Subscribe",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Path", "Path", "Path", GH_ParamAccess.item, "cubes");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Cubes", "Cubes", "Status", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override async void SolveInstance(IGH_DataAccess DA)
        {
            var subscribe = false;
            var path = "cubes";

            if (!DA.GetData(0, ref subscribe)) return;
            DA.GetData(1, ref path);

            var config = Environment.GetEnvironmentVariable("firebaseConfig");

            _client = Firebase.GetOrCreateClient(_client, config);

            if (_client != null && subscribe)
            {
                try
                {
                    //FirebaseStream = await Client.OnAsync(path, (sender, args, context) => {
                    //    Slots = ParseFirebaseSlots(args.Data);
                    //});
                    
                    var firebaseResponse = await _client.GetAsync(path);
                    if (firebaseResponse.StatusCode == HttpStatusCode.OK)
                    {
                        _cubes = ParseFirebaseCubes(firebaseResponse.Body);
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }
            else if (!subscribe)
            {
                //FirebaseStream.Dispose();
            }

            DA.SetDataList(0, _cubes);
        }

        private static List<Box> ParseFirebaseCubes(string data)
        {
            var cubes = new List<Box>();
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(data);
            foreach (var key in jsonData.Keys)
            {
                var _data = jsonData[key]["position"] as JObject;
                var point = _data.ToObject<Dictionary<string, int>>();
                var plane = new Plane(
                    new Point3d(point["x"], point["z"], point["y"]), new Vector3d(0.0, 0.0, 1.0));
                cubes.Add(new Box(plane, new List<Point3d>{new Point3d(point["x"] + 25, point["z"] + 25, point["y"] + 25), new Point3d(point["x"] - 25, point["z"] - 25, point["y"] - 25)}));
            }

            return cubes;
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
        public override Guid ComponentGuid => new Guid("13254b70-5786-4593-98fa-a6f1279ab336");

        private IFirebaseClient _client = null;

        private List<Box> _cubes;
        //private EventStreamResponse FirebaseStream;
    }
}