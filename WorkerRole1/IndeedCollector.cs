using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WorkerRole1
{
    class IndeedCollector : IJobCollectable
    {
        private static readonly string URL_TEMPLATE = @"http://www.indeed.com/jobs?q=_FREETEXT_&start=_PAGE_";

        private static readonly string PAGE_NUM_PLACEHOLDER = "_PAGE_";
        private static readonly string FREE_TEXT_PLACEHOLDER = "_FREETEXT_";

        public string CollectJobs(string i_FreeText)
        {
            return CollectJobs(i_FreeText, 0);
        }

        public string CollectJobs(string i_FreeText, int pageNum)
        {
            string urlEncodedFreeText = HttpUtility.UrlPathEncode(i_FreeText);
            string indeedUrl = URL_TEMPLATE.Replace(PAGE_NUM_PLACEHOLDER, pageNum.ToString()).Replace(FREE_TEXT_PLACEHOLDER, urlEncodedFreeText);

            // Download the entire HTML of the page
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.GetEncoding("windows-1255");

            webClient.Headers[HttpRequestHeader.Host] = "www.indeed.com";
            webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";
            string htmlCode = webClient.DownloadString(indeedUrl);

            // Extract the relevant jobs from the HTML
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlCode);
            HtmlNode rateNode = htmlDocument.DocumentNode.SelectSingleNode("//td[@id='resultsCol']");

            List<HtmlNode> unwantedNodes = new List<HtmlNode>()
            {
                htmlDocument.DocumentNode.SelectSingleNode("//div[@id='resumePromo']"),
                htmlDocument.DocumentNode.SelectSingleNode("//div[@id='bjobalerts']"),
                htmlDocument.DocumentNode.SelectSingleNode("//div[@class='pagination']"),
                htmlDocument.DocumentNode.SelectSingleNode("//div[@class='related_searches']")
            };

            string jobsHtml = (rateNode == null) ? string.Empty : rateNode.InnerHtml;

            foreach (HtmlNode unwantedNode in unwantedNodes)
            {
                if (unwantedNode != null)
                {
                    jobsHtml = jobsHtml.Replace(unwantedNode.InnerHtml, string.Empty);
                }
            }

            return jobsHtml.Replace("\"/", "\"http://www.indeed.com/").Replace("'/", "'http://www.indeed.com/");
        }
    }
}
