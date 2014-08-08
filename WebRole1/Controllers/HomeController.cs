using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using WebRole1.Models;

namespace WebRole1.Controllers
{
    public class HomeController : Controller
    {
        public static readonly int JOB_SITE_COUNT = Enum.GetValues(typeof(JobSiteEnum)).Length;
        public readonly string search = "SearchString";

        public ActionResult Index()
        {
            ViewBag.numOfJobs = JOB_SITE_COUNT;
            ViewBag.results = RedisCacheManager.GetFromCache((string)Session[search]);
            ViewBag.expectingResult = ResultsContainerListener.ExpectingNewResult;

            return View();
        }

        [HttpPost]
        public ActionResult SearchJob(string searchString)
        {
            //string searchString = ISearchString.First<String>();
            Session[search] = searchString;

            // Load sites dynamically only if results are not already in cache
            ResultsContainerListener.ExpectingNewResult = (searchString != null) ?
                                                          !RedisCacheManager.AddIfNotExists(searchString) :
                                                          false;

            return RedirectToAction("Index");
        }

        public ActionResult FindNewResults()
        {
            // Fetch new results from container and parse to readable JSON
            string resultsJson = string.Empty;

            if (Session[search] != null)
            {
                Dictionary<string, string> newResults = ResultsContainerListener.CheckForNewResult((string)Session[search]);
                resultsJson = newResults.Count() >= JOB_SITE_COUNT ? "results found" : string.Empty;
            }

            return Json(new { Results = resultsJson }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}