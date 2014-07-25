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
            HtmlNode rateNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='MainContent_JobList_jobList']");

            string relevantScripts =
@"<link href=""http://www.drushim.co.il/GetResource?css=4f45908dd84a2a019978f849f392ba5e"" rel=""stylesheet"" type=""text/css"" media=""all"" />";
            string jobsHtml = relevantScripts + rateNode.InnerHtml;

            return jobsHtml;
        }
    }
}
