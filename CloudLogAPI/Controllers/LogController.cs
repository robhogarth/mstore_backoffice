using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace CloudLogAPI.Controllers
{
    public class LogController : Controller
    {
        // GET: Log
        public ActionResult Index()
        {
            return View();
        }

        [System.Web.Http.HttpPost]
        public void Post([FromBody] LogMessage lMessage )
        {

        }
    }
}