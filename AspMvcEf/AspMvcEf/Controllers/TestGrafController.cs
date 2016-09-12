using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspMvcEf.Controllers
{
    public class TestGrafController : Controller
    {
        // GET: Default
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Graf()
        {
            var con = new AspMvcEf.Models.Entities();
            var model = con.Test_Result;
            var musers = model.GroupBy(u => u.Test_User).Select(x => new { k = x.Key, v = x.Count() }).ToDictionary(x=>x.k);
            
            return View();
        }

    }


}