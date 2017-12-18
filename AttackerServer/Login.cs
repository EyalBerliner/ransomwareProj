using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AttackerServer
{
    public static class Login
    {
        [FunctionName("Login")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "HttpTriggerCSharp/name/{name}/pass/{pass}")]HttpRequestMessage req,
            string name, string pass, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            log.Info("Got this name and pass " + name + ":" + pass);
            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
        }
    }
}
