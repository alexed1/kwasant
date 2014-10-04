using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KwasantCore.Managers.APIManagers.Serializers.Json;


namespace Utilities
{
    public class JsonPackager
    {
        private JsonSerializer jsonSerializer;

        public JsonPackager()
        {
            jsonSerializer = new JsonSerializer();
        }
        public string Pack(object dataObject)
        {
            return jsonSerializer.Serialize(dataObject);
        }
    }
}
