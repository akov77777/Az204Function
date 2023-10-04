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

namespace Az204Function
{
    public static class SendMessageToQueueFunction
    {
        [FunctionName("SendMessageToQueueFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req
            //, ILogger log
            )
        {
            try
            {
                //log.LogInformation("C# HTTP trigger function processed a request. SendMessageToQueueFunction");
                var request = await GetQueueRequest(req);

                // Instantiate a QueueClient to create and interact with the queue
                QueueClient queueClient = new QueueClient(
                    new Uri($"https://{request.StorageAccountName}.queue.core.windows.net/{request.QueueName}"),
                    new DefaultAzureCredential());
                await queueClient.CreateIfNotExistsAsync();

                QueueClient outputQueueClient = new QueueClient(
                    new Uri($"https://{request.StorageAccountName}.queue.core.windows.net/{request.OutputQueueName}"),
                    new DefaultAzureCredential());
                await outputQueueClient.CreateIfNotExistsAsync();

                await queueClient.SendMessageAsync(request.Message);
                SendReceipt receipt = await queueClient.SendMessageAsync($"Message with receipt: {request.Message}");

                await outputQueueClient.SendMessageAsync(request.Message);
                SendReceipt outputReceipt = await outputQueueClient.SendMessageAsync($"Message with receipt: {request.Message}");

                StringBuilder sb = new StringBuilder();
                PeekedMessage[] peekedMessages = await outputQueueClient.PeekMessagesAsync(maxMessages: 10);
                if (peekedMessages.Length > 0)
                {
                    sb.AppendLine("Peeked Mesages");
                    foreach (var message in peekedMessages)
                    {
                        sb.AppendLine(message.MessageText);
                    }
                }

                QueueMessage[] messages = await outputQueueClient.ReceiveMessagesAsync(maxMessages: 10);
                if (messages.Length > 0)
                {
                    sb.AppendLine("Queued Mesages");
                    foreach (var message in messages)
                    {
                        sb.AppendLine(message.MessageText);
                    }
                }

                string responseMessage = string.IsNullOrEmpty(request.Message)
                    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                    : $"Hello, {request.Message}. This HTTP triggered function executed successfully. Addituinial info: {sb}";

                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex) 
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static async Task<QueueRequest> GetQueueRequest(HttpRequest req)
        {
            string message = await GetMessageFromQuery(req, "message");
            string storageAccountName = await GetMessageFromQuery(req, "storageAccountName");
            string queueName = await GetMessageFromQuery(req, "queueName");
            string outputQueueName = await GetMessageFromQuery(req, "outputQueueName");

            return new QueueRequest(message, storageAccountName, queueName, outputQueueName);
        }

        private static async Task<string> GetMessageFromQuery(HttpRequest req, string paramName)
        {
            string message = req.Query[paramName];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            return message ?? data?.message;
        }
    }
}
