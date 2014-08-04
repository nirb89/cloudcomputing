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
    class AllJobsCollector : IJobCollectable
    {
        private static readonly string URL_TEMPLATE = @"http://www.alljobs.co.il/searchresultsguest.aspx?page=_PAGE_&position=&type=&freetxt=_FREETEXT_&city=&region=";

        private static readonly string PAGE_NUM_PLACEHOLDER = "_PAGE_";
        private static readonly string FREE_TEXT_PLACEHOLDER = "_FREETEXT_";

        public string CollectJobs(string i_FreeText)
        {
            return CollectJobs(i_FreeText, 1);
        }

        public string CollectJobs(string i_FreeText, int pageNum)
        {
            string urlEncodedFreeText = HttpUtility.UrlPathEncode(i_FreeText);
            string allJobsUrl = URL_TEMPLATE.Replace(PAGE_NUM_PLACEHOLDER, pageNum.ToString()).Replace(FREE_TEXT_PLACEHOLDER, urlEncodedFreeText);
            
            // Download the entire HTML of the page
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.GetEncoding("windows-1255");

            webClient.Headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            webClient.Headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.8,he;q=0.6";
            webClient.Headers[HttpRequestHeader.Host] = "www.alljobs.co.il";
            webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";
            string htmlCode = webClient.DownloadString(allJobsUrl);

            // Extract the relevant jobs from the HTML
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlCode);
            HtmlNode rateNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='divOpenBoardContainer']");
            
            if (rateNode == null)
            {
                rateNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='divOrganicContainer']");
            }

            string jobsHtml = (rateNode == null) ? 
                string.Empty : 
                rateNode.InnerHtml.Replace("/images", "http://www.alljobs.co.il/images");

            return jobsHtml.Replace("\"/", "\"http://www.alljobs.co.il/").Replace("'/", "'http://www.alljobs.co.il/");
        }
    }
}
