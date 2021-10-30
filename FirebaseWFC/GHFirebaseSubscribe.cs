using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using FireSharp.Interfaces;
using FireSharp.Response;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Rhino.Geometry;

namespace FirebaseWFC
{
    public class GHFirebaseSubscribe : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHFirebaseSubscribe()
            : base("Subscribe", "Subscribe",
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "Status", "Status", GH_ParamAccess.item);
            pManager.AddPointParameter("Slots", "Slots", "Slots", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override async void SolveInstance(IGH_DataAccess DA)
        {
            var subscribe = false;
            if (!DA.GetData(0, ref subscribe)) return;

            var config = Environment.GetEnvironmentVariable("firebaseConfig");

            _client = Firebase.GetOrCreateClient(_client, config);
            
            if (_client != null && subscribe)
            {
                try
                {
                    //FirebaseStream = await Client.OnAsync("slots", (sender, args, context) => {
                    //    Slots = ParseFirebaseSlots(args.Data);
                    //});
                    var firebaseResponse = await _client.GetAsync("slots");
                    if (firebaseResponse.StatusCode == HttpStatusCode.OK)
                    {
                        _slots = ParseFirebaseSlots(firebaseResponse.Body);
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
            
            DA.SetDataList(1, _slots);
        }

        private static List<Point3d> ParseFirebaseSlots(string data)
        {
            var jsonData = JsonConvert.DeserializeObject<List<int[]>>(data);

            return jsonData.Select(element => new Point3d { X = element[0], Y = element[1], Z = element[2] }).ToList();
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
        public override Guid ComponentGuid => new Guid("cbaf115b-22cd-474a-880c-8a12f6baa19b");

        private IFirebaseClient _client = null;
        private List<Point3d> _slots;
        //private EventStreamResponse FirebaseStream;
    }
}