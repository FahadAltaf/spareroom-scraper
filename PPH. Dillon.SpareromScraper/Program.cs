using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.ServiceConfigurators;

namespace PPH.Dillon.SpareromScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            var topshelfExitCode = HostFactory.Run(x =>
            {
                x.Service<Heartbeat>((Action<ServiceConfigurator<Heartbeat>>)(s =>
                {
                    s.ConstructUsing<Heartbeat>(h => new Heartbeat());
                    s.WhenStarted<Heartbeat>(h => h.Start());
                    s.WhenStopped<Heartbeat>(h => h.Stop());
                }));
                x.RunAsLocalSystem();
                x.SetServiceName("spareroom_renewal_service");
                x.SetDisplayName("spareroom_renewal_service");
                x.SetDescription("spareroom_renewal_service");
            });
            Environment.ExitCode = (int)Convert.ChangeType(topshelfExitCode, topshelfExitCode.GetType());
        }
    }
}
