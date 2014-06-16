using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KwasantCore.Managers.APIManager.Serializers.Json;

namespace KwasantCore.Managers.APIManager.Packagers.DataTable
{
    public class DataTablesPackager
    {
        private JsonSerializer jsonSerializer;

        public DataTablesPackager()
        {
            jsonSerializer = new JsonSerializer();
        }
        public string Pack(object dataObject) {
            return jsonSerializer.Serialize(dataObject);
        }
    }
}
