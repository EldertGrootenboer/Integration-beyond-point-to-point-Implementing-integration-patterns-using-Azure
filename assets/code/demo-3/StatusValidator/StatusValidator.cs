using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EPH.Functions
{
    public static class StatusValidator
    {
        [FunctionName("StatusValidator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string status = data?.Status?.Value;
            DateTime purchaseDate = DateTime.Parse((string)data?.PurchaseDate);
            string assetType = data?.AssetType?.Value;
            var isStatusOk = true;

            switch (assetType)
            {
                case "Phone":
                    // 2 years
                    isStatusOk = status != "Retired" || purchaseDate > DateTime.Now.Subtract(new TimeSpan(730, 0, 0, 0));
                    break;
                case "Laptop":
                    // 3 years
                    isStatusOk = status != "Retired" || purchaseDate > DateTime.Now.Subtract(new TimeSpan(1095, 0, 0, 0));
                    break;
                default:
                    // 5 years
                    isStatusOk = status != "Retired" || purchaseDate > DateTime.Now.Subtract(new TimeSpan(1825, 0, 0, 0));
                    break;
            }

            return new ObjectResult(isStatusOk);
        }
    }
}
