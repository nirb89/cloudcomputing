using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WebRole1.Models
{
    public static class WebStorageManager
    {
        public static readonly string WebConnectionName = "WebStorageConnectionString";
        public static readonly string ResultsContainerName = "results";
        public static readonly string WorkQueueName = "workqueue";

        public static readonly CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(
                RoleEnvironment.GetConfigurationSettingValue(WebConnectionName));

        public static List<string> DownloadResultBlobs(string freeTextInput)
        {
            // Create the blob client. 
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(ResultsContainerName);

            // Create the container if it doesn't already exist.
            blobContainer.CreateIfNotExists();

            List<string> resultsList = new List<string>();

            // Loop over items within the container and output the length and URI.
            foreach (IListBlobItem item in blobContainer.ListBlobs(null, false))
            {
                CloudBlockBlob blob = item as CloudBlockBlob;

                if (blob != null && blob.Name.StartsWith(freeTextInput))
                {
                    // Extract job data for specific free text input
                    resultsList.Add(blob.DownloadText());
                }
            }

            return resultsList;
        }

        public static void InsertToQueue(string freeTextInput)
        {
            if (isTextInputInResultsTable(freeTextInput))
            {
                // If the input has already been calculated in the past, don't add it to the queue
                return;
            }

            // Create the queue client.
            CloudQueueClient queueClient = StorageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference(WorkQueueName);

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExists();

            foreach (JobSiteEnum jobSite in Enum.GetValues(typeof(JobSiteEnum)))
            {
                // Create a message and add it to the queue.
                CloudQueueMessage message = new CloudQueueMessage((int) jobSite + ":" + freeTextInput);
                queue.AddMessage(message);
            }
        }

        private static bool isTextInputInResultsTable(string freeTextInput)
        {
            // Create the blob client. 
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(ResultsContainerName);

            // Create the container if it doesn't already exist.
            blobContainer.CreateIfNotExists();

            return blobContainer.GetBlockBlobReference(freeTextInput + ":0").Exists();
        }
    }
}