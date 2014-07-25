using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace WorkerRole1
{
    class DrushimCollector : IJobCollectable
    {
        private static readonly string URL_TEMPLATE = @"http://www.drushim.co.il/jobs/search/_FREETEXT_/?page=_PAGE_";

        private static readonly string PAGE_NUM_PLACEHOLDER = "_PAGE_";
        private static readonly string FREE_TEXT_PLACEHOLDER = "_FREETEXT_";

        public string CollectJobs(string i_FreeText)
        {
            string urlEncodedFreeText = HttpUtility.UrlPathEncode(i_FreeText);
            string jobMasterUrl = URL_TEMPLATE.Replace(PAGE_NUM_PLACEHOLDER, "0").Replace(FREE_TEXT_PLACEHOLDER, urlEncodedFreeText);

            // Download the entire HTML of the page
            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Host] = "www.drushim.co.il";
            webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";
            string htmlCode = webClient.DownloadString(jobMasterUrl);

            // Extract the relevant jobs from the HTML
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlCode);
            HtmlNode scriptsNode = htmlDocument.DocumentNode.SelectSingleNode("//head[@id='Head1']");
            HtmlNode rateNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='MainContent_JobList_jobList']");
            HtmlNode unwantedNode = htmlDocument.DocumentNode.SelectSingleNode("//tr[@class='pagingRow']");

            string relevantScripts = (scriptsNode == null) ? 
                string.Empty : 
                scriptsNode.InnerHtml;

            string fullJobsTable = (rateNode == null) ?
                string.Empty :
                    (unwantedNode == null) ?    
                        rateNode.InnerHtml : 
                        rateNode.InnerHtml.Replace(unwantedNode.InnerHtml, string.Empty);

            string jobsHtml = relevantScripts + fullJobsTable;

            return jobsHtml;
        }
    }
}
