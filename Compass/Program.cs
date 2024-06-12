using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support;
using System.Collections.Generic;
using System.Threading;

namespace demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // driver init
            FirefoxBinary binary = new FirefoxBinary();
            FirefoxOptions options = new FirefoxOptions();//optional
            options.BrowserExecutableLocation = @"C:\Program Files\Mozilla Firefox\firefox.exe";
            IWebDriver driver;

            //data
            string email = @"compass9012@gmail.com";
            string password = @"Takehome1234567890";
            string loginBtn = @"uc-corpNav-loginBtn";
            string baseCompassUrl = @"https://www.compass.com"; 
            string tempurl = @"{0}/search/sales/nyc/";
            string url = string.Format(tempurl, baseCompassUrl);

            using (driver = new FirefoxDriver(@"C:\bin", options))
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
                driver.Navigate().GoToUrl(url);

                // Log in
                driver.FindElement(By.ClassName(loginBtn)).Click();
                driver.FindElement(By.XPath("//button[.='Buyer / Seller']")).Click();
                driver.FindElement(By.XPath("//div[.='Continue with Email']")).Click();
                
                driver.FindElement(By.Name("email")).SendKeys(email);
                driver.FindElement(By.Id("continue")).Click();

                driver.FindElement(By.Name("password")).SendKeys(password);
                driver.FindElement(By.Id("continue")).Click();
                Thread.Sleep(5000);

                // revist Url with credentials
                driver.Navigate().GoToUrl(url);

                // click to open Locations
                driver.FindElement(By.Name("Locations")).Click();

                // select Manhattan
                var boroughs = driver.FindElements(By.ClassName("uc-searchBarTooAccordionMenuItem-headlineLabel"));
                foreach(var borough in boroughs)
                {
                    string name = borough.GetAttribute("innerText");
                    if (name.Contains("Manhattan"))
                    {
                        borough.Click();
                        break;
                    }
                }

                // click all Manhattan
                var neighborhoods = driver.FindElements(By.ClassName("searchFilter-label"));
                foreach(var neighborhood in neighborhoods)
                {
                    string name = neighborhood.GetAttribute("innerText");
                    if (name.Contains("All Manhattan"))
                    {
                        neighborhood.Click();
                        break;
                    }
                }

                // click to close Locations
                driver.FindElement(By.Name("Locations")).Click();

                // click Price
                var prices = driver.FindElements(By.XPath("//span[.='Price']"));
                prices[1].Click();  //double hack. should find the 2nd Price link by attribute and determine if it needs 1 or 2 click for decending sort
                Thread.Sleep(1000);
                prices[1].Click();

                // get rows from first page
                var table = driver.FindElement(By.ClassName("ag-body-viewport"));
                var rows = table.FindElements(By.ClassName("ag-row"));

                string targetHood= string.Empty;
                HashSet<string> hoodsSeen = new HashSet<string>();
                List<string> top5Urls = new List<string>();
                int i = 0;

                //loop through each row
                foreach (var row in rows)
                {
                    // get link address
                    var buildingName = row.FindElement(By.ClassName("lolTable-cols-buildingName"));
                    var address = buildingName.GetAttribute("innerText");
                    var reflink = buildingName.FindElement(By.TagName("a"));
                    var link = reflink.GetAttribute("href");

                    // loop through each column in a row
                    var columns = row.FindElements(By.ClassName("ag-cell-no-focus"));
                    foreach(var column in columns)
                    {
                        // get the neighborhood
                        if(column.GetAttribute("colid").Equals("listing.location.neighborhood"))
                        {
                            targetHood = column.GetAttribute("innerText");
                            break;
                        }
                    }

                    // if first time hood save it's link
                    if(!hoodsSeen.Contains(targetHood))
                    {
                        hoodsSeen.Add(targetHood);
                        i++;
                        top5Urls.Add(link);
                        if (i == 5)
                        {
                            break;
                        }
                    }
                }

                foreach(var topUrl in top5Urls)
                {
                    Console.WriteLine(topUrl);
                }
                
                driver.Quit();
            }


        }
    }
}
