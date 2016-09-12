using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace AspMvcEf.Controllers
{
    public class TestScheduleController : Library.Controllers.BaseController<Models.Test_Schedule>
    {
        
        public virtual ActionResult Run(int id)
        {
            Models.Scheduler s = HttpContext.Application["Scheduler"] as Models.Scheduler;
            var item = model.Find(id);

            if (item != null)
            {
                Mutex mutex = new Mutex(false, "Task_" + item.Id);
                if (mutex.WaitOne(1000, false))
                {
                    Thread t = new Thread(() => s.Task(con, item, mutex, false));
                    t.Start();
                    TempData["Result"] = "Spustam test " + id;
                }
                else
                {
                    TempData["Result"] = "Spusteny test stale bezi " + id;
                }

            }
            
            return RedirectToAction("Index");
        }
        
        
    }
}