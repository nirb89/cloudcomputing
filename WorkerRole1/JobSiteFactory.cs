using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    static class JobSiteFactory
    {
        /// <summary>
        /// Factory for job collector. Each collector knows how to extract jobs from a specific website.
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public static IJobCollectable GetCollector(JobSiteEnum jobType)
        {
            IJobCollectable jobCollector;

            switch (jobType)
            {
                case JobSiteEnum.AllJobs:
                    jobCollector = new AllJobsCollector();
                    break;
                case JobSiteEnum.JobMaster:
                    jobCollector = new JobMasterCollector();
                    break;
                case JobSiteEnum.Drushim:
                default:
                    jobCollector = new DrushimCollector();
                    break;
            }

            return jobCollector;
        }
    }
}
