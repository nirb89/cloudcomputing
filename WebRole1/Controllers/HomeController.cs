using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebRole1.Models;

namespace WebRole1.Controllers
{
    public class HomeController : Controller
    {

        public readonly string search = "SearchString";

        public ActionResult Index()
        {
            string searchString = (string) Session[search];
            ViewBag.results = WebStorageManager.DownloadResultBlobs(searchString);

            return View();
        }

        [HttpPost]
        public ActionResult SearchJob(string searchString)
        {
            //string searchString = ISearchString.First<String>();
            Session[search] = searchString;

            WebStorageManager.InsertToQueue(searchString);

            return RedirectToAction("Index");
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