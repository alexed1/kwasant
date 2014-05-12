using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

namespace UtilitiesLib.Logging
{
    public sealed class LogEventEntity : TableServiceEntity
    {
        public LogEventEntity()
        {
            var now = DateTime.UtcNow;
            PartitionKey = string.Format("{0:yyyy-MM HH:mm:ss}", now);
            RowKey = string.Format("{0:dd HH:mm:ss.fff}-{1}",
                                    now, Guid.NewGuid());
            HostName = System.Environment.MachineName;
        }

        public string Identity { get; set; }
        public string ThreadName { get; set; }
        public string LoggerName { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Domain { get; set; }
        public string RoleInstance { get; set; }
        public string DeploymentId { get; set; }
        public string HostName { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            var returnStr = new StringBuilder();

            returnStr.Append("".PadRight(80, '*'));
            returnStr.Append(Environment.NewLine);

            var props = GetType().GetProperties().Where(p => p.Name != "Exception").ToList();

            var maxPropLength = props.Max(p => p.Name.Length);

            var formatString = "{0,-" + (maxPropLength + 5) + "} {1}\n";
            foreach (var prop in props)
            {
                var str = String.Format(formatString, prop.Name, prop.GetValue(this));
                returnStr.Append(str);
            }

            var currException = Exception;
            while (currException != null)
            {
                returnStr.Append(Environment.NewLine);
                returnStr.AppendLine(currException.Message);
                returnStr.AppendLine(currException.StackTrace);
                currException = currException.InnerException;
            }

            returnStr.Append("".PadRight(80, '*'));
            returnStr.Append(Environment.NewLine);
            return returnStr.ToString();
        }
    }
}
