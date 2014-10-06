using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ExampleWebAsync
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

    public class JobRunner
    {
        private readonly AutoResetEvent _waiter = new AutoResetEvent(false);
        private int _lastJob;

        private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> _jobs
            = new ConcurrentDictionary<int, TaskCompletionSource<string>>();  

        static JobRunner()
        {
            Instance = new JobRunner();
            Task.Run(() => Instance.Run());
        }
        public static JobRunner Instance { get; set; }

        public Task<string> StartJob(int jobNumber)
        {
            var job = new TaskCompletionSource<string>();
            _jobs.TryAdd(jobNumber, job);
            _lastJob = jobNumber;
            /* drop file in folder */
            Task.Delay(TimeSpan.FromSeconds(4)).ContinueWith(_ => _waiter.Set());
            return job.Task;
        }

        private void Run()
        {
            /* start monitoring message queue */
            _waiter.WaitOne();
            _jobs[_lastJob].SetResult("done in background");
        }

    }
}
