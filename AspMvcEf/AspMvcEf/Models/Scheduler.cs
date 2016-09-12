using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace AspMvcEf.Models
{
    public class Scheduler
    {
        Thread t;

        public Scheduler()
        {
            t = new Thread(() => Runnable());
        }

        ~Scheduler()
        {
            try
            {
                t.Interrupt();

            }
            catch (Exception e) { }
            try
            {
                t.Abort();

            }
            catch (Exception e) { }
        }

        public void Start()
        {
            if (t.ThreadState != ThreadState.Running)
            {
                t.Start();
            }
        }

        /*
         * Thread #1
         */
        private void Runnable()
        {
            while (true)
            {

                using (Models.Entities con = new Models.Entities())
                {
                    Test_Schedule model = con.Test_Schedule.FirstOrDefault(x => x.Flag_Start != 1 && x.Time_Start < DateTime.Now);
                    if (model != null)
                    {
                        Mutex mutex = new Mutex(false, "Task_" + model.Id);
                        if (mutex.WaitOne(1000, false))
                        {
                            Task(con, model, mutex, true);
                        }
                    }
                }
                Thread.Sleep(1000 * 1);
            }
        }


        public void Task(Models.Entities con, Test_Schedule model, Mutex mutex, bool saveFlags)
        {
            try
            {
                if (saveFlags)
                {
                    model.Flag_Start = 1;
                    con.SaveChanges();
                }

                var db = model.Test_Db;
                var url = model.Test_Url;
                var users = model.Test_Url.Test_User;

                foreach (var tgRef in model.Test_Group.Test_Group_Ref)
                {
                    foreach (var user in users)
                    {
                        var result = new AspMvcEf.Models.Test_Result()
                        {
                            Id = 0,
                            Test_Schedule_Id = model.Id,
                            Test_Group_Id = tgRef.Test_Group_Id,
                            Test_Id = tgRef.Test_Id,
                            Flag_Ok = null,
                            Result = null,
                            Time_Start = DateTime.Now,
                            Time_Stop = null
                        };
                        try
                        {
                            //Models.SeleniumTest.Run(tgRef.Test.Test_Type.Type, tgRef.Test.Insurance_Id, user.Username, user.Password, url.Url, db.CisConnectionString, db.WebConnectionString);
                            result.Result = "";
                            result.Flag_Ok = 1;
                        }
                        catch (Exception e)
                        {
                            result.Result = e.ToString();
                            result.Flag_Ok = 0;
                        }
                        result.Time_Stop = DateTime.Now;
                        con.Entry(result).State = System.Data.Entity.EntityState.Added;
                        con.SaveChanges();
                    }
                }

                if (saveFlags)
                {
                    model.Flag_Stop = 1;
                    con.SaveChanges();
                }
            }
            finally
            {
                mutex.Close();
            }
        }
    }
}