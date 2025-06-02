using FatecDemandaDownloaderNetFramework.Models;
using FatecDemandaDownloaderNetFramework.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static FatecDemandaDownloaderNetFramework.Utils;

namespace FatecDemandaDownloaderNetFramework.Services
{
    public class DemandDownloaderService
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;
        private HomeDemandasHelper _homeHelper;
        private SelectFatecHelper _fatecHelper;

        public const string fileOutputFileName = "fatec_demanda_data.csv";

        private IWebDriver CreateWebDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-extensions");
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);

            var stringArguments = options.Arguments.Any() ? $" arguments: {string.Join(", ", options.Arguments)}" : "no arguments";

            Log($"Starting the web driver with browser '{options.BrowserName}', version: '{options.BrowserVersion}', with {stringArguments}");
            return new ChromeDriver(options);
        }

        public async Task ExecuteAsync()
        {
            _driver = CreateWebDriver();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));

            _fatecHelper = new SelectFatecHelper(_driver);
            _homeHelper = new HomeDemandasHelper(_driver);

            Log($"Starting the Fatec demand data download ");
            var totalStopwatch = Stopwatch.StartNew(); // Início do tempo total


            await _homeHelper.GoToHomePageAsync();

            AcceptCookiesIfPresent();

            var availableSemesters = _homeHelper.GetAllSemesterOptions(true);

            const string targetFatecOriginal = "Mauá";

            var data = new List<DemandaFatec>();

            foreach (var semester in availableSemesters)
            {
                var semesterData = await ProcessSemesterAsync(semester, targetFatecOriginal);
                data.AddRange(semesterData);
            }
            totalStopwatch.Stop();
            Log($"Execution total time: {FormatElapsedTime(totalStopwatch.Elapsed)}");

            CsvExporterService.SaveToCsv(data, fileOutputFileName);

            Log("Data saved successfully!");
        }

        private void AcceptCookiesIfPresent()
        {
            bool accepted = _homeHelper.ClickOnConsentCookiesButton();
            Log(accepted ? "Cookies accepted." : "Cookies not shown or already accepted.");
        }

        private async Task<List<DemandaFatec>> ProcessSemesterAsync(SemesterOption semester, string targetFatec)
        {
            var stopwatch = Stopwatch.StartNew();
            var list = new List<DemandaFatec>();

            Log($"Processing semester {semester.semester}/{semester.year}... ", false, true);

            var semesterSelect = _homeHelper.GetSemesterSelectElement();
            semesterSelect.SelectByValue(semester.value);

            _wait.Until(d => d.FindElement(By.CssSelector("#formDemanda button.btn[type='send']"))).Click();
            _wait.Until(d => d.FindElements(By.CssSelector("#FATEC option")).Count > 2);

            var formattedFatec = ParseFatecName(targetFatec);
            var normalized = NormalizeText(targetFatec);

            var fatecSelect = _fatecHelper.GetFatecSelectElement();

            var match = fatecSelect.Options
                .FirstOrDefault(opt => NormalizeText(opt.Text).Contains(normalized));

            if (match == null)
            {
                Log($"Fatec '{targetFatec}' not found.", true, false);
                await _homeHelper.GoToHomePageAsync();
                return list;
            }

            fatecSelect.SelectByValue(match.GetAttribute("value"));

            _wait.Until(d => d.FindElement(By.CssSelector("#formDemanda > button"))).Click();
            _wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);

            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            foreach (var row in rows)
            {
                var cols = row.FindElements(By.TagName("td"));
                if (cols.Count != 5) continue;

                if (!int.TryParse(cols[2].Text.Trim(), out int inscritos)) continue;
                if (!int.TryParse(cols[3].Text.Trim(), out int vagas) || vagas == 0) continue;

                var demanda = new DemandaFatec
                {
                    Fatec = formattedFatec,
                    Ano = semester.year,
                    Semestre = semester.semester,
                    Curso = CapitalizeText(cols[0].Text.Trim()),
                    Periodo = CapitalizeText(cols[1].Text.Trim()),
                    Inscritos = inscritos,
                    Vagas = vagas,
                    Demanda = inscritos / (double)vagas
                };

                list.Add(demanda);
            }

            stopwatch.Stop();
            Log($"Finished in {FormatElapsedTime(stopwatch.Elapsed)}", true, false);

            await _homeHelper.GoToHomePageAsync();

            return list;
        }

    }
}
