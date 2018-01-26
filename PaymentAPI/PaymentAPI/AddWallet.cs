
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace PaymentAPI
{
    public static class AddWallet
    {

        [FunctionName("AddWallet")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            string id = req.Query["id"];
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id = id ?? data?.id;
            if (string.IsNullOrEmpty(id))
            {
                return new BadRequestObjectResult("Please pass a id on the query string or in the request body");
            }
            string wallet = await createPaymentEntity(id, log);
            if (wallet == null)
            {
                return new BadRequestObjectResult("failed to create new wallet");
            }
            return (ActionResult)new OkObjectResult(wallet);
        }

        public static async Task<string> createPaymentEntity(string customerId, TraceWriter log)
        {
            string url = "https://block.io/api/v2/get_new_address/?api_key=";
            url += GetEnvironmentVariable("BitCoinAPIKey");
            url += "&label=";
            url += customerId;
            HttpClient client = new HttpClient();
            var response = await client.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                log.Info("C# Request for new wallet failed, HTTP req failed");
                return null;
            }
            string jsonContent = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);
            string walletId = data?.data?.address ?? data.address;
            if (string.IsNullOrEmpty(walletId))
            {
                log.Info("C# Request for new wallet failed, Strcture not as expected");
                return null;
            }
            return walletId;
        }

        public static string GetEnvironmentVariable(string name)
        {
            if (name == "BitCoinAPIKey")
            {
                return "baf3-e84a-09c0-eb0d";
            }
            return System.Environment.GetEnvironmentVariable(name, System.EnvironmentVariableTarget.Process);
        }
    }
}
