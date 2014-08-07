using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StackExchange.Redis;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WebRole1.Models
{
    public class RedisCacheManager
    {
        private static readonly int     REDIS_PORT   = 6379;
        private static readonly string  REDIS_URL = RoleEnvironment.GetConfigurationSettingValue("Redis.Host");
        private static readonly string  REDIS_PASS = RoleEnvironment.GetConfigurationSettingValue("Redis.Password");


        private static readonly int EXPIRATION_TIME = 20;

        private static ConnectionMultiplexer _cacheConnection = null;

        public static void connectIfNotConnected()
        {
            if (_cacheConnection == null)
            {
                ConfigurationOptions config = new ConfigurationOptions()
                {
                    EndPoints = { { REDIS_URL, REDIS_PORT } },
                    KeepAlive = 180,
                    Password = REDIS_PASS,
                    AllowAdmin = true,
                    SyncTimeout = 20000
                };

                _cacheConnection = ConnectionMultiplexer.Connect(config);
            }
        }

        public static void disconnectIfConnected()
        {
            if (_cacheConnection != null)
            {
                _cacheConnection.Close();
                _cacheConnection = null;
            }
        }

        public static bool AddIfNotExists(String key)
        {
            bool existsInCache = false;

            if (key == null)
            {
                return true;
            }

            connectIfNotConnected();

            IDatabase cache = _cacheConnection.GetDatabase();
            string keyNameInCache;

            foreach (JobSiteEnum jobSite in Enum.GetValues(typeof(JobSiteEnum)))
            {
                keyNameInCache = (int)jobSite + ":" + key;

                existsInCache = cache.KeyExists(keyNameInCache);

                if (!existsInCache)
                {
                    WebStorageManager.InsertJobToQueue(key, jobSite);
                }
            }

            disconnectIfConnected();

            return existsInCache;
        }

        public static int GetCountFromCache(String key)
        {
            List<string[]> valuesFromKey = GetFromCache(key);
            return valuesFromKey.Count;
            
        }

        public static List<string[]> GetFromCache(String key)
        {
            if (key == null)
            {
                return new List<string[]>();
            }

            connectIfNotConnected();

            IDatabase cache = _cacheConnection.GetDatabase();

            string data = null;
            string keyNameInCache;

            List<string[]> jobsData = new List<string[]>();

            foreach (JobSiteEnum jobSite in Enum.GetValues(typeof(JobSiteEnum)))
            {
                int jobSiteInt = (int)jobSite;
                keyNameInCache = jobSiteInt + ":" + key;

                try
                {
                    if (cache.KeyExists(keyNameInCache))
                    {
                        data = cache.StringGet(keyNameInCache);
                        jobsData.Add(new string [] {jobSiteInt.ToString(), Enum.GetName(typeof(JobSiteEnum), jobSiteInt), data} );
                    }
                    else
                    {
                        // First time seeing this data, add to cache
                        data = WebStorageManager.DownloadSingleResultBlob(key, jobSite);

                        if (data != null)
                        {
                            cache.StringSetAsync(keyNameInCache, data, TimeSpan.FromMinutes(EXPIRATION_TIME));
                            jobsData.Add(new string[] {jobSiteInt.ToString(), Enum.GetName(typeof(JobSiteEnum), jobSiteInt), data});
                        }
                    }
                }
                catch
                {
                    // Do nothing, but don't throw exception
                }
            }

            disconnectIfConnected();

            return jobsData;
        }
    }
}