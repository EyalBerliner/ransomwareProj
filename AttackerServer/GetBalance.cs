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
    public static class GetBalance
    {
        [FunctionName("GetBalance")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "id", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            id = id ?? data?.id;

            if (string.IsNullOrEmpty(id))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a id on the query string or in the request body");
            }
            double balance = await getCustomerBalance(id, log);
            if (balance < 0)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Failed to get balance.");
            }
            return req.CreateResponse(HttpStatusCode.OK, balance);
        }

        public static async Task<double> getCustomerBalance(string customerId, TraceWriter log)
        {
            string url = "https://block.io/api/v2/get_address_balance/?api_key=";
            url += GetEnvironmentVariable("BitCoinAPIKey");
            url += "&labels=";
            url += customerId;
            HttpClient client = new HttpClient();
            var response = await client.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                log.Info("C# Request for new wallet failed, HTTP req failed");
                return -1;
            }
            string jsonContent = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);
            string balance = data?.data?.available_balance ?? data.available_balance;
            double balanceValue = 0.0;
            if (string.IsNullOrEmpty(balance) || !double.TryParse(balance, out balanceValue))
            {
                log.Info("C# Request for new wallet failed, Strcture not as expected");
                return -1;
            }
            return balanceValue;
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
