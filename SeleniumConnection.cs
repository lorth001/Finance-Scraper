/*
 * Author:  Luke Orth
 * Date:    12.17.2020
 * Version: 0.0.1
 * Updates: Initial version
 * 
 * About:   This "Connection" class is responsible for instantiation a new Selenium web driver.
 *          It also configures the options for this web driver.
*/

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager.DriverConfigs.Impl;

namespace FinancialScraper
{
    public class SeleniumConnection
    {
        private ChromeOptions Options;  // options for web driver
        private string CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

        /* Add default download directory */
        private void DriverOptions()
        {
            Options = new ChromeOptions();
            Options.AddUserProfilePreference("download.default_directory", CurrentDirectory);
        }

        /* Instantiate new web driver with options and return driver */
        public IWebDriver Get(string url)
        {
            DriverOptions();    // call function to get options
            new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());

            IWebDriver Driver = new ChromeDriver(Options); // instantiate driver

            Driver.Url = url;   // add desired URL to the web driver

            return Driver;
        }
    }
}
