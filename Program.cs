/*
 * Author:  Luke Orth
 * Date:    12.17.2020
 * Version: 0.0.1
 * Updates: Initial version
 * 
 * About:   The purpose of this program is to track my financial spending.  This is accomplished
 *          by interacting via Selenium with the websites of financial institutions I do 
 *          business with (AMEX, etc.).  The Selenium web driver will download the .CSV file
 *          which contains my transactions for the current month.  This file will then be parsed 
 *          and its data inserted into a custom database.  This data will then be displayed on
 *          a custom website for me to view.
*/

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace FinancialScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            //GetAmex amex = new GetAmex();
            //amex.Go();
            ParseAmex parseAmex = new ParseAmex();
            parseAmex.Go();
        }
    }
}
