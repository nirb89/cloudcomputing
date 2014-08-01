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
    class JobMasterCollector : IJobCollectable
    {
            private static readonly string URL_TEMPLATE = @"http://www.jobmaster.co.il/code/check/search.asp?currPage=_PAGE_&key=_FREETEXT_";

            private static readonly string PAGE_NUM_PLACEHOLDER = "_PAGE_";
            private static readonly string FREE_TEXT_PLACEHOLDER = "_FREETEXT_";

            public string CollectJobs(string i_FreeText)
            {
                return CollectJobs(i_FreeText, 0);
            }

            public string CollectJobs(string i_FreeText, int pageNumber)
            {
                string urlEncodedFreeText = HttpUtility.UrlPathEncode(i_FreeText);
                string jobMasterUrl = URL_TEMPLATE.Replace(PAGE_NUM_PLACEHOLDER, (pageNumber + 1).ToString()).Replace(FREE_TEXT_PLACEHOLDER, urlEncodedFreeText);

                // Download the entire HTML of the page
                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.GetEncoding("windows-1255");

                webClient.Headers[HttpRequestHeader.Host] = "www.jobmaster.co.il";
                webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";
                string htmlCode = webClient.DownloadString(jobMasterUrl).Replace(System.Environment.NewLine, string.Empty);


                int jobStartIndex = htmlCode.IndexOf("<TABLE BORDER=0 CELLSPACING=0 CELLPADDING=0 WIDTH=\"100%\">");
                htmlCode = htmlCode.Insert(jobStartIndex + 6, " id='relevantJobsId' ");

                // Extract the relevant jobs from the HTML
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlCode);
                HtmlNode rateNode = htmlDocument.DocumentNode.SelectSingleNode("//table[@id='relevantJobsId']");
                string jobsHtml = (rateNode == null) ?
                    string.Empty :
                    rateNode.InnerHtml.Replace("../../images", "http://www.jobmaster.co.il/images");

                return jobsHtml;
            }
    }
}
