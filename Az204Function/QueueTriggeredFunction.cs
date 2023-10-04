using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace Az204Function
{
    //public class QueueTriggeredFunction : QueueClientBase
    //{

    //    public QueueTriggeredFunction(IConfiguration configuration) : base(configuration)
    //    {
    //    }

    //    [FunctionName("QueueTriggeredFunction")]
    //    public async Task Run(
    //        [QueueTrigger("az204queue", Connection= "SendMessageToQueueFunction_ConnectionString")] string notification,
    //        [Queue("az204queue-output", Connection = "SendMessageToQueueFunction_ConnectionString")] IAsyncCollector<string> queue,
    //        ILogger log)
    //    {
    //        //var queueClient = await GetQueueClient("az204queue-output");
    //        //await queueClient.SendMessageAsync("PROCESSED TRIGGERED FUNCTION");

    //        await queue.AddAsync($"{notification} + OUTPUT!");
    //        log.LogInformation("C# HTTP trigger function processed a request.");

    //        //return new OkObjectResult($"{notification} + OUTPUT! RESPONSE");
    //    }
    //}
}
