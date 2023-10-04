using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Queues;
using Azure.Identity;
using Azure.Storage.Queues.Models;
using System.Text;
using Az204Function.Models;
using System.Linq;
using Azure.Storage;
using Microsoft.Extensions.Configuration;

namespace Az204Function
{
    public abstract class QueueClientBase
    {
        protected readonly IConfiguration _configuration;

        public QueueClientBase(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected async Task<QueueClient> GetQueueClient(string queueName)
        {
            //Don't pass connectionString as query parameter from Url

            var connectionString = _configuration.GetValue<string>("SendMessageToQueueFunction_ConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _configuration.GetConnectionStringOrSetting("SendMessageToQueueFunction");
            }

            //connectionString = _configuration["ConnectionString"];
            QueueClient queueClient = new QueueClient(connectionString, queueName);
            await queueClient.CreateIfNotExistsAsync();

            //Another way
            //var key = "Dt2AgOMu848b5aur8CkayGz2W7sFC5nrGEaNh/g9BunbOZuvb07MwyRugYrkEpif7ByA9J9wzzcB+AStHltwfw==";
            //var accName = "cloudshell191292145";
            //var storageSharedKeyCredential = new StorageSharedKeyCredential(accName, key);
            //var uri = new Uri("https://cloudshell191292145.queue.core.windows.net/az204queue");
            //queueClient = new QueueClient(uri, storageSharedKeyCredential);
            //await queueClient.CreateIfNotExistsAsync();

            return queueClient;
        }

    }
}
