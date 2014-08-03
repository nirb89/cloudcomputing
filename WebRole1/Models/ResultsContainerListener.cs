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

        //private static CloudBlobClient m_BlobClient = WebStorageManager.StorageAccount.CreateCloudBlobClient();
        //private static CloudBlobContainer m_BlobContainer = m_BlobClient.GetContainerReference(WebStorageManager.ResultsContainerName);
        private static List<string[]> m_CacheCurrentList = new List<string[]>();
        private static int m_CacheCurrentListCount = m_CacheCurrentList.Count();

        public static List<string[]> CheckForNewResult(string freeText)
        {
            List<string[]> newBlobItemsData = new List<string[]>();

            List<string[]> newResults = findNewResults(freeText);

            ExpectingNewResult = false;

            return newResults;
        }

        private static List<string[]> findNewResults(string freeText)
        {

            List<string[]> updatedCacheList = RedisCacheManager.GetFromCache(freeText);

            /*
            if (updatedCacheList == null)
            {
                return null;
            }
            */

            int updatedListCount = updatedCacheList.Count();

            /*
            if (updatedListCount == m_CacheCurrentListCount)
            {
                return null;
            }
            */

            /*
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

            IEnumerable<CloudBlockBlob> newResults = blobsWithAttributes.OrderByDescending(blob => blob.Properties.LastModified)
                .Take(updatedListCount - m_CacheCurrentListCount);
            */
            // Set the updated list as the new curent list, as well as its updated size
            m_CacheCurrentList = updatedCacheList;
            m_CacheCurrentListCount = updatedListCount;

            return updatedCacheList;

        }
    }
}