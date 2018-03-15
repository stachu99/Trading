using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataScraper.Services
{
    public static class MyExtentions
    {
        //IWebElement FindElement(By by);

        /// <summary>
        /// Same as FindElement only returns null when not found instead of an exception.
        /// </summary>
        /// <param name=”driver”>current browser instance</param>
        /// <param name=”by”>The search string for finding element</param>
        /// <returns>Returns element or null if not found</returns>
        public static IWebElement FindElementSafe(this IWebDriver driver, By by)
        {
            try
            {
                return driver.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }


        //
        // Summary:
        //     Repeatedly applies this instance's input value to the given function until one
        //     of the following occurs:
        //     the function returns neither null nor false the function throws an exception
        //     that is not in the list of ignored exception types the timeout expires
        //
        // Parameters:
        //   condition:
        //     A delegate taking an object of type T as its parameter, and returning a TResult.
        //
        // Type parameters:
        //   TResult:
        //     The delegate's expected return type.
        //
        // Returns:
        //     The delegate's return value.
        public static TResult UntilSafe<TResult>(this WebDriverWait webDriverWait, Func<IWebDriver, TResult> condition)
        {
            try
            {
               return webDriverWait.Until<TResult>(condition);
            }
            catch (Exception)
            {
                return default(TResult);
            }
        }
    }
}
