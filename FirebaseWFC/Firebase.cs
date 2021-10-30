using System.Collections.Generic;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Newtonsoft.Json;

namespace FirebaseWFC
{
    public class Firebase
    {
        public static IFirebaseClient GetOrCreateClient(IFirebaseClient client, string config)
        {
            if (client != null || string.IsNullOrEmpty(config)) return client;

            var configDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(config);
            var firebaseConfig = new FirebaseConfig
            {
                AuthSecret = configDict["apiKey"],
                BasePath = $"https://{configDict["authDomain"]}/"
            };
            return new FirebaseClient(firebaseConfig);
        }
    }
}