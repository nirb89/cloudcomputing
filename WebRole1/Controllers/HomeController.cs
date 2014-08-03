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

        public readonly string search = "SearchString";
        public static int value = 0;
        

        public ActionResult Index()
        {
            string searchString = (string) Session[search];
            ViewBag.results = RedisCacheManager.GetFromCache(searchString);
            ViewBag.expectingResult = ResultsContainerListener.ExpectingNewResult;

            return View();
        }

        [HttpPost]
        public ActionResult SearchJob(string searchString)
        {
            //string searchString = ISearchString.First<String>();
            Session[search] = searchString;

            RedisCacheManager.AddIfNotExists(searchString);

            ResultsContainerListener.ExpectingNewResult = true;

            return RedirectToAction("Index");
        }

        public ActionResult FindNewResults()
        {
            // Fetch new results from container and parse to readable JSON
            List<string[]> newResults = ResultsContainerListener.CheckForNewResult((string)Session[search]);
            string resultsJson = newResults.Count() > 0 ? JsonConvert.SerializeObject(newResults) : string.Empty;

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