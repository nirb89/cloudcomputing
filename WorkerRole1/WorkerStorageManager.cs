using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    class WorkerStorageManager
    {
        public static readonly string WebConnectionName = "WorkerStorageConnectionString";
        public static readonly string ResultsContainerName = "results";
        public static readonly string WorkQueueName = "workqueue";

        public static readonly CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
                RoleEnvironment.GetConfigurationSettingValue(WebConnectionName));

        public static void AddToResultBlob(string freeTextInput, JobSiteEnum jobSiteType, string jobResult)
        {
            // Create the blob client.
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(ResultsContainerName);

            // Create the container if it doesn't already exist.
            blobContainer.CreateIfNotExists();

            // Message is unique -> "[job site type]:[free text input]"
            string resultBlockName = (int)jobSiteType + ":" + freeTextInput;

            // Upload a text blob
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(resultBlockName);
            blob.UploadTextAsync(jobResult);

            // Log a message that can be viewed in the diagnostics tables called WADLogsTable
            System.Diagnostics.Trace.WriteLine("Transferring result to 'results' blob.");
        }

        public static CloudQueueMessage ReadFromQueue()
        {
            // Create the queue client
            CloudQueueClient queueClient = StorageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue
            CloudQueue queue = queueClient.GetQueueReference(WorkQueueName);

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExists();

            // Get the next message
            CloudQueueMessage retrievedMessage = queue.GetMessage();

            if (retrievedMessage != null)
            {
                // Process the message in less than 30 seconds, and then delete the message
                queue.DeleteMessage(retrievedMessage);
            }

            return retrievedMessage;
        }
    }
}
