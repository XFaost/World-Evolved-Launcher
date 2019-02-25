using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFS.Class.Diss.RPC
{
    class EventList
    {
        public static string getEventName(int id)
        {
            dynamic dynJson = JsonConvert.DeserializeObject(ExtractResource.AsString("NFS.Class.Diss.events.json"));

            foreach (var item in dynJson)
            {
                if (item.id == id)
                {
                    return item.trackname;
                }
            }

            return "EVENT:" + id;
        }

        public static string getEventType(int id)
        {
            dynamic dynJson = JsonConvert.DeserializeObject(ExtractResource.AsString("NFS.Class.Diss.cars.json"));

            foreach (var item in dynJson)
            {
                if (item.id == id)
                {
                    return item.type;
                }
            }

            return "gamemode_freeroam";
        }
    }
}
