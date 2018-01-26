using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AttackerServer
{
    public static class AddWallet
    {
        [FunctionName("AddWallet")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger AddWallet function processed a request.");

            // parse query parameter

            string id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "id", true) == 0)
                .Value;

            // Get request body
            string content = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(content);

            // Set name to query string or body data
            id = id ?? data?.id;

            if (string.IsNullOrEmpty(id))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a id on the query string or in the request body");
            }
            string wallet = await createPaymentEntity(id, log);
            if (string.IsNullOrEmpty(wallet))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Failed to create a wallet.");
            }
            return req.CreateResponse(HttpStatusCode.OK, wallet);
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
