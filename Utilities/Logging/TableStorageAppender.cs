using System;
using System.Data.Services.Client;
using System.IO;
using log4net.Appender;
using log4net.Core;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

namespace UtilitiesLib.Logging
{
    public class TableStorageAppender : AppenderSkeleton
    {
        private readonly string _logFileName = Environment.GetEnvironmentVariable("HOMEDRIVE") + Path.Combine(Environment.GetEnvironmentVariable("HOMEPATH"), "KwasantLog", "log.txt");
        private bool _isLoggingToDisk;
        private TableServiceContext _dataContext;

        public string ConnectionStringKey
        {
            get
            {
                return CloudConfigurationManager.GetSetting("LoggingConnectionString");
                ;
            }
        }

        public string TableName
        {
            get
            {
                return CloudConfigurationManager.GetSetting("LoggingTableName");
            }
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            var connectionString = ConnectionStringKey;
            if (String.IsNullOrEmpty(connectionString))
            {
                _isLoggingToDisk = true;
                return;
            }

            var account = CloudStorageAccount.Parse(connectionString);

            var tableClient = account.CreateCloudTableClient();
            tableClient.GetTableReference(TableName).CreateIfNotExists();

            _dataContext = tableClient.GetTableServiceContext();
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var entity = new LogEventEntity
                {
                    RoleInstance = "1", //RoleEnvironment.CurrentRoleInstance.Id,
                    DeploymentId = "1", //RoleEnvironment.DeploymentId,
                    Message = loggingEvent.RenderedMessage,
                    Level = loggingEvent.Level.Name,
                    LoggerName = loggingEvent.LoggerName,
                    Domain = loggingEvent.Domain,
                    ThreadName = loggingEvent.ThreadName,
                    Exception = loggingEvent.ExceptionObject,
                };
                if (_isLoggingToDisk)
                {
                    Directory.CreateDirectory(new FileInfo(_logFileName).Directory.FullName);
                    using (var fs = File.AppendText(_logFileName))
                    {
                        fs.Write(entity.ToString());
                    }
                    return;
                }


                _dataContext.AddObject(TableName, entity);
                _dataContext.SaveChanges();
            }
            catch (DataServiceRequestException drex)
            {
                ErrorHandler.Error("Could not write log entry", drex);
            }
        }
    }
}
