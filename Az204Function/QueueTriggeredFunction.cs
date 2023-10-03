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

namespace Az204Function
{
    public class QueueTriggeredFunction
    {
        [FunctionName("QueueTriggeredFunction")]
        //[QueueOutput("az204queue-output")]
        public static async Task<IActionResult> Run(
            [QueueTrigger("az204queue")] string notification,
            [Queue("az204queue-output")] IAsyncCollector<string> queue,
            ILogger log)
        {
            await queue.AddAsync($"{notification} + OUTPUT!");
            log.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult($"{notification} + OUTPUT! RESPONSE");
        }
    }
}
