using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerRole1 entry point called");

            IJobCollectable jobCollector;
            CloudQueueMessage queueMessage;
            JobSiteEnum jobSite;

            string[] jobMessage;
            string freeTextInput;
            string jobsHtmlData;

            while (true)
            {
                queueMessage = WorkerStorageManager.ReadFromQueue();

                while (queueMessage != null)
                {
                    // Parse message into the job site type, and the free text input
                    jobMessage = queueMessage.AsString.Split(new char[] { ':' });
                    jobSite = (JobSiteEnum) int.Parse(jobMessage[0]);
                    freeTextInput = jobMessage[1];

                    // Fetch collector by type and retrieve jobs from site
                    jobCollector = JobSiteFactory.GetCollector(jobSite);
                    jobsHtmlData = jobCollector.CollectJobs(freeTextInput);

                    // Upload job data to results container
                    WorkerStorageManager.AddToResultBlob(freeTextInput, jobSite, jobsHtmlData);

                    // Repeat until queue is empty
                    queueMessage = WorkerStorageManager.ReadFromQueue();
                }

                Thread.Sleep(10000);
                Trace.TraceInformation("Working");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
