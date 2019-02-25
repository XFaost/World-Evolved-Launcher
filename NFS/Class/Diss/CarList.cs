using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFS.Class.Diss.RPC
{
    class CarList
    {
        public static string getCarName(string id)
        {
            dynamic dynJson = JsonConvert.DeserializeObject(ExtractResource.AsString("NFS.Class.Diss.cars.json"));

            foreach (var item in dynJson)
            {
                if (item.carid == id)
                {
                    return item.carname;
                }
            }

            return "Traffic Car (" + id + ")";
        }
    }
}
