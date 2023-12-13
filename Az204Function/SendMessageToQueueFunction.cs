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
    public class SendMessageToQueueFunction : QueueClientBase
    {
        //https://www.infoworld.com/article/3628229/how-to-work-with-azure-queue-storage-in-csharp.html
        //https://learn.microsoft.com/en-us/azure/storage/queues/storage-quickstart-queues-dotnet?tabs=passwordless%2Croles-azure-portal%2Cenvironment-variable-windows%2Csign-in-azure-cli
        //https://learn.microsoft.com/en-us/azure/storage/common/storage-account-keys-manage?tabs=azure-portal

        //https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-azure-functions-csharp?tabs=isolated-process
        //https://azurelessons.com/how-to-access-app-setting-azure-functions/

        public SendMessageToQueueFunction(IConfiguration configuration) : base(configuration)
        {
        }

        //If we use it with QueueTriggeredFunction
        //it conficts because the same az204queue is trigger
        //http://localhost:7187/api/SendMessageToQueueFunction?message=test&queueName=az204queue&outputQueueName=az204queue-output
        [FunctionName("SendMessageToQueueFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req
            //, ILogger log
            )
        {
            try
            {
                //log.LogInformation("C# HTTP trigger function processed a request. SendMessageToQueueFunction");
                var request = await GetQueueRequest(req);

                // Instantiate a QueueClient to create and interact with the queue
                var queueClient = await GetQueueClient(request.QueueName);

                var outputQueueClient = await GetQueueClient(request.OutputQueueName);
                

                await queueClient.SendMessageAsync(request.Message);
                SendReceipt receipt = await queueClient.SendMessageAsync($"Message with receipt: {request.Message}");

                //await outputQueueClient.SendMessageAsync($"{request.Message} OUTPUT");
                //SendReceipt outputReceipt = await outputQueueClient.SendMessageAsync($"Message with receipt: {request.Message} OUTPUT");

                StringBuilder sb = await GetMessages(outputQueueClient);

                string responseMessage = string.IsNullOrEmpty(request.Message)
                    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                    : $"Hello, {request.Message}. This HTTP triggered function executed successfully. Additionial info: {sb}";

                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex) 
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static async Task<StringBuilder> GetMessages(QueueClient outputQueueClient)
        {
            StringBuilder sb = new StringBuilder();
            PeekedMessage[] peekedMessages = await outputQueueClient.PeekMessagesAsync(maxMessages: 10);
            if (peekedMessages.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Peeked Mesages");
                peekedMessages.Select(x => x.MessageText).ToList().ForEach(i => sb.AppendLine(i));
            }

            QueueMessage[] queueMessages = await outputQueueClient.ReceiveMessagesAsync(maxMessages: 10);
            if (queueMessages.Length > 0)
            {
                sb.AppendLine("Queued Mesages");
                queueMessages.Select(x => x.MessageText).ToList().ForEach(i => sb.AppendLine(i));
            }

            return sb;
        }

        private static async Task<QueueRequest> GetQueueRequest(HttpRequest req)
        {
            string message = await GetMessageFromQuery(req, "message");
            string queueName = await GetMessageFromQuery(req, "queueName");
            string outputQueueName = await GetMessageFromQuery(req, "outputQueueName");

            return new QueueRequest(message, queueName, outputQueueName);
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