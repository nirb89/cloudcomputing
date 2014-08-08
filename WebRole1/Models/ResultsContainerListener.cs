using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Blob;

namespace WebRole1.Models
{
    public class ResultsContainerListener
    {
        public static bool ExpectingNewResult = false;

        private static CloudBlobClient m_BlobClient = WebStorageManager.StorageAccount.CreateCloudBlobClient();
        private static CloudBlobContainer m_BlobContainer = m_BlobClient.GetContainerReference(WebStorageManager.ResultsContainerName);
        private static IEnumerable<IListBlobItem> m_BlobCurrentList = new List<IListBlobItem>();
        private static int m_BlobCurrentListCount = 0;

        public static Dictionary<string, string> CheckForNewResult(string freeText)
        {
            Dictionary<string, string> newBlobItemsData = new Dictionary<string, string>();

            IEnumerable<CloudBlockBlob> newResults = findNewBlobs(freeText);

            if (newResults != null)
            {
                foreach (CloudBlockBlob blob in newResults)
                {
                    // Extract and parse the data file name and code file name from the result text
                    string[] jobTypeAndSearchText = blob.Name.Split(new char[] { ':' });
                    string jobType = jobTypeAndSearchText[0];
                    string searchText = jobTypeAndSearchText[1];

                    if (searchText.Equals(freeText))
                    {
                        newBlobItemsData.Add(jobType, blob.DownloadText());
                    }

                }

                // Results were found, set flag to false
                ExpectingNewResult = false;
            }

            return newBlobItemsData;
        }

        private static IEnumerable<CloudBlockBlob> findNewBlobs(string freeText)
        {
            IEnumerable<IListBlobItem> updatedBlobList = m_BlobContainer.ListBlobs();

            if (updatedBlobList == null)
            {
                return null;
            }

            int updatedListCount = updatedBlobList.Count();

            if (updatedListCount <= m_BlobCurrentListCount)
            {
                return null;
            }

            List<CloudBlockBlob> blobsWithAttributes = new List<CloudBlockBlob>();

            // Sort the updated list by its modified time
            foreach (IListBlobItem blobItem in updatedBlobList)
            {
                CloudBlockBlob blob = blobItem as CloudBlockBlob;
                if (blob != null)
                {
                    blob.FetchAttributes();
                    blobsWithAttributes.Add(blob);
                }
            }

            var results = from   blob in blobsWithAttributes
                          where  blob.Name.Contains(freeText)
                          select blob;

            IEnumerable<CloudBlockBlob> newResults = results.ToList();

            // Set the updated list as the new curent list, as well as its updated size
            m_BlobCurrentList = updatedBlobList;
            m_BlobCurrentListCount = updatedListCount;

            return newResults;

        }
    }
}