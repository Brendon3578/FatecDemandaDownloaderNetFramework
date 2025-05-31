using FatecDemandaDownloaderNetFramework.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FatecDemandaDownloaderNetFramework.Pages
{
    public class HomeDemandasHelper
    {

        private static IWebDriver _webDriver;
        private static string HomePageUrl = "https://vestibular.fatec.sp.gov.br/demanda/";

        public static string AnoSemesterSelectorName = "ano-sem";
        private static string CookiesConsentButtonId = "consent-cookies";

        public HomeDemandasHelper(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        async public Task GoToHomePageAsync()
        {
            await _webDriver.Navigate().GoToUrlAsync(HomePageUrl);
        }

        public bool ClickOnConsentCookiesButton()
        {
            try
            {
                var wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(2));
                wait.Until(d => d.FindElement(By.Id(CookiesConsentButtonId)));

                var cookieConsentButton = _webDriver.FindElement(By.Id(CookiesConsentButtonId));
                if (cookieConsentButton.Displayed)
                {
                    cookieConsentButton.Click();
                    return true;
                }
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            return false;
        }

        public SelectElement GetSemesterSelectElement()
        {
            var semesterSelectElement = _webDriver.FindElement(By.Name(AnoSemesterSelectorName));
            return new SelectElement(semesterSelectElement);
        }

        public List<SemesterOption> GetAllSemesterOptions(bool shouldReverOrder = false)
        {

            var semesterSelect = GetSemesterSelectElement();


            var availableSemesters = semesterSelect.Options
                .Where(option => option.GetAttribute("value") != null && option.GetAttribute("value").Length <= 5)
                .Select(option =>
                {
                    var value = option.GetAttribute("value");

                    int year = int.Parse(value.Substring(0, 4));     // "2025"
                    int semester = int.Parse(value.Substring(4, 1)); // "1"

                    return new SemesterOption
                    {
                        semester = semester,
                        year = year,
                        value = value
                    };

                }).ToList();

            if (shouldReverOrder)
                availableSemesters.Reverse();

            return availableSemesters;
        }
    }
}
