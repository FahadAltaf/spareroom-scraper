using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace PPH.Dillon.SpareromScraper
{
    public class Heartbeat
    {
        private System.Timers.Timer _heartbeat;

        public Heartbeat()
        {
            this._heartbeat = new System.Timers.Timer(3000.0)
            {
                AutoReset = true
            };
            this._heartbeat.Elapsed += new ElapsedEventHandler(this._heartbeat_Elapsed);
        }



        private void _heartbeat_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Runing script at ", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            _heartbeat.Interval = 1000 * 60 * 60;
            this._heartbeat.Stop();

            RenewAds();
            Console.Clear();
            Console.WriteLine("Last run at {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            Console.WriteLine("waiting for next event...");
            this._heartbeat.Start();
        }

        private void RenewAds()
        {
            string username = ConfigurationManager.AppSettings.Get("username");
            string password = ConfigurationManager.AppSettings.Get("password");
            var pages = 0;
            if (int.TryParse(ConfigurationManager.AppSettings.Get("pages"), out pages))
            {
                Console.WriteLine("Initializing...");
                ChromeOptions options = new ChromeOptions();
                options.AddArguments((IEnumerable<string>)new List<string>()
                    {
                      "--silent-launch",
                      "--no-startup-window",
                      "no-sandbox",
                      "headless",
        });
                ChromeDriverService defaultService = ChromeDriverService.CreateDefaultService();
                defaultService.HideCommandPromptWindow = true;
                using (IWebDriver driver = (IWebDriver)new ChromeDriver(/*defaultService, options*/))
                {
                    try
                    {
                        Console.WriteLine("Loging in...");
                        string url = "http://spareroom.co.uk/";
                        driver.Navigate().GoToUrl(url);

                        Thread.Sleep(3000);
                        var loginBtn = driver.FindElement(By.XPath("/html/body/header/div[2]/div/div/div[3]/div[2]/a"));
                        loginBtn.Click();

                        Thread.Sleep(2000);
                        var userNameField = driver.FindElement(By.XPath("//*[@id=\"loginemail\"]"));
                        userNameField.SendKeys(ConfigurationManager.AppSettings.Get("username"));
                        Thread.Sleep(1000);

                        var passwordField = driver.FindElement(By.XPath("//*[@id=\"loginpass\"]"));
                        passwordField.SendKeys(ConfigurationManager.AppSettings.Get("password"));
                        Thread.Sleep(1000);

                        var submitBtn = driver.FindElement(By.XPath("//*[@id=\"sign-in-button\"]"));
                        submitBtn.Click();
                        Thread.Sleep(3000);
                        Console.WriteLine("Logged in...");
                        for (int p = 0; p < 5; p++)
                        {
                            driver.Navigate().GoToUrl("https://www.spareroom.co.uk/flatshare/mylistings.pl?offset=" + (p * 10));
                            int m = 0;
                            var links = driver.FindElements(By.ClassName("myListing-link__activate"));
                            for (int i = 0; i < links.Count; i++)
                            {
                                if (!links[i].GetAttribute("class").Contains("deactivated") && links[i].Text == "Renew")
                                {
                                    try
                                    {
                                        links[i].Click();
                                        Thread.Sleep(2000);
                                        var confirmBtn = driver.FindElement(By.XPath("//*[@id=\"maincontent\"]/div[4]/div[" + m + "]/div[1]/div[2]/a"));
                                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                                        js.ExecuteScript("arguments[0].click()", confirmBtn);
                                        Console.WriteLine("Ad renewed");
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Ad already renewed");
                                    }

                                    driver.Navigate().GoToUrl("https://www.spareroom.co.uk/flatshare/mylistings.pl?offset=" + (p * 10));
                                    links = driver.FindElements(By.ClassName("myListing-link__activate"));

                                }
                                if (i % 2 == 0)
                                    m += 1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }

                }
            }
            else
                Console.WriteLine("Invalid configurations");
           
        }

        public void Start()
        {
            this._heartbeat.Start();
        }

        public void Stop()
        {
            this._heartbeat.Stop();
        }
    }
}
