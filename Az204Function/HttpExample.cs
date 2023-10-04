using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Az204Function
{
    public static class HttpExample
    {
        //https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio
        //For local testing append the query string ?name=<YOUR_NAME> to this URL and run the request. 
        //After publishing to Azure in the address bar in the browser, append the string /api/HttpExample?name=Functions to the base URL and run the request.



        //https://portal.azure.com/learn.docs.microsoft.com
        //https://learn.microsoft.com/en-us/training/modules/introduction-to-azure-app-service/7-create-html-web-app?source=learn


        //Timer trigger https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=python-v2%2Cisolated-process%2Cnodejs-v4&pivots=programming-language-csharp

        [FunctionName("HttpExample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req
            //, ILogger log
            )
        {
            //log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully. 2023-04-10-21:35";

            return new OkObjectResult(responseMessage);
        }
    }
}
