using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace FatecDemandaDownloaderNetFramework.Pages
{
    public class SelectFatecHelper
    {
        private static IWebDriver _webDriver;
        public static string FatecSelectId = "FATEC";

        public SelectFatecHelper(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public SelectElement GetFatecSelectElement()
        {
            var fatecSelectElement = _webDriver.FindElement(By.Id(FatecSelectId));
            return new SelectElement(fatecSelectElement);
        }


    }
}
