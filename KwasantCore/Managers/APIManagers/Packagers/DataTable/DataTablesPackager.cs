using KwasantCore.Managers.APIManagers.Serializers.Json;

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
