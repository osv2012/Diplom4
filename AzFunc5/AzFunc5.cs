using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzFunc5
{
    public static class AzFunc5
    {
        [FunctionName("AzFunc5")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            DayDataRepository DayDatas = new DayDataRepository(log);

            string mode = req.Query["mode"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            mode = mode ?? data?.mode;

            if (!string.IsNullOrEmpty(mode))
            {
                if ("0" == mode.Trim())
                {
                    DayDatas.AllDelete(log);
                }
                else
                {
                    DayDatas.AllDelete(log);
                    DayDatas.AllLoad(log);
                }
            }

            string responseMessage = string.IsNullOrEmpty(mode)
                ? "This HTTP triggered function executed successfully. Pass a mode in the query string or in the request body for a personalized response."
                : $"Hello, {mode}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
