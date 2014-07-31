using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    interface IJobCollectable
    {
        string CollectJobs(string i_FreeText);

        string CollectJobs(string i_FreeText, int pageNumber);
    }
}
