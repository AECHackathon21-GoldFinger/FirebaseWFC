using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Drawing;
using FireSharp.Interfaces;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Rhino.Geometry;

namespace FirebaseWFC
{
    public class GHGetScore : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHGetScore()
            : base("Get Score", "Get Score",
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
            pManager.AddTextParameter("Path", "Path", "Path", GH_ParamAccess.item, "user");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "Status", "Status", GH_ParamAccess.item);
            pManager.AddTextParameter("Users", "Users", "Users", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override async void SolveInstance(IGH_DataAccess DA)
        {
            var subscribe = false;
            var path = "users";

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
                        _users = ParseFirebaseUsers(firebaseResponse.Body);
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

            DA.SetDataList(1, _users);
        }

        private static string ParseFirebaseUsers(string data)
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(data);
            var _data = "USERS:";
            foreach (var key in jsonData.Keys)
            {
                _data += $"\n{key}:";
                foreach (var dataKey in jsonData[key].Keys)
                {
                    _data += $"\n  {dataKey}: {jsonData[key][dataKey]}";
                }

                _data += "\n";
            }

            return _data;
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
        public override Guid ComponentGuid => new Guid("5830874e-cb14-4bd7-bc59-71f8876f8824");

        private IFirebaseClient _client = null;

        private string _users;
        //private EventStreamResponse FirebaseStream;
    }
}