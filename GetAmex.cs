/*
 * Author:  Luke Orth
 * Date:    12.17.2020
 * Version: 0.0.1
 * Updates: Initial version
 * 
 * About:   This "Amex" class handles all interactions with the AMEX website via Selenium.
 *          It is called from the main "Program", instantiates a new Selenium web driver
 *          via the "Connection" class, and uses the web driver to navigate the AMEX site and
 *          download the relevant .CSV file.
*/

using OpenQA.Selenium;
using System;
using System.IO;

namespace FinancialScraper
{
    class GetAmex
    {
        private IWebDriver Driver;  // web driver
        private string filename = System.AppDomain.CurrentDomain.BaseDirectory + @"\activity.csv";

        /* Create and return the full URL path for the AMEX website */
        private string TargetUrl()
        {
            DateTime todaysDate = DateTime.Now.Date;            // current date
            string dd = todaysDate.Day.ToString();              // day
            string mm = todaysDate.Month.ToString();            // month
            string yy = todaysDate.Year.ToString();             // year

            string today = $"{yy}-{mm}-{dd}";                   // today's date

            DateTime weekAgoDate = DateTime.Now.AddDays(-7);    // date one week ago
            dd = weekAgoDate.Day.ToString();                    // day
            mm = weekAgoDate.Month.ToString();                  // month
            yy = weekAgoDate.Year.ToString();                   // year 

            string weekAgo = $"{yy}-{mm}-{dd}";

            string url = $"https://global.americanexpress.com/activity/search?from={weekAgo}&to={todaysDate}";   // create full URL path

            return url;
        }

        /* Connect to the AMEX website */
        private void Connect()
        {
            string url = TargetUrl();
            SeleniumConnection connection = new SeleniumConnection();           // instantiate new Selenium web driver
            Driver = connection.Get(url);                                       // specify URL to use
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60); // require the driver to wait at least 60 seconds before timing out on any command
        }

        /* Login to the website */
        private void Login()
        {
            Driver.FindElement(By.Id("eliloUserID")).SendKeys("USERNAME");          // username
            Driver.FindElement(By.Id("eliloPassword")).SendKeys("PASSWORD");    // password
            Driver.FindElement(By.Id("loginSubmit")).Click();                       // click "Login"
        }

        /* Download .CSV file with all transactions for the current month */
        private void DownloadFile()
        {
            Driver.FindElement(By.CssSelector(".btn.btn-fluid")).Click();                   // click the "Search" button
            if (Driver.FindElements(By.CssSelector(".btn.dls-icon-download")).Count < 1)    // check if the "download" button is present
            {
                Console.WriteLine($"No results found for the chosen dates");                // if not present, it's likely there weren't any transactions in the selected date range
                return;                                                                     // exit the function
            }
            Driver.FindElement(By.CssSelector(".btn.dls-icon-download")).Click();                                           // click the "download" icon
            Driver.FindElement(By.XPath("//label[@for='axp-activity-download-body-selection-options-type_csv']")).Click();  // select the "CSV" option
            Driver.FindElement(By.Id("axp-activity-download-body-checkbox-options-includeAll")).Click();                    // check box to include all transaction details
            Driver.FindElement(By.CssSelector(".btn.btn-primary.btn-block")).Click();                                       // click the "Download" button
        }

        /* Close the browser and quit Selenium */
        private void CloseAndQuit()
        {
            System.Threading.Thread.Sleep(5000);
            Driver.Close(); // close browser
            Driver.Quit();  // quit Selenium
        }

        /* Controller function */
        public void Go()
        {
            File.Delete(filename);  // delete previous downloaded file (if it exists)
            Connect();              // connect to AMEX site
            Login();                // login to site
            DownloadFile();         // download .CSV file
            CloseAndQuit();         // exit browser and quit     
        }
    }
}
