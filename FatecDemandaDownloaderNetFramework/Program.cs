using FatecDemandaDownloaderNetFramework.Pages;
using FatecDemandaDownloaderNetFramework.Services;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatecDemandaDownloaderNetFramework
{

    class Program
    {
        public static IWebDriver WebDriver = null;
        public static HomeDemandasHelper HomeDemandasHelper = null;

        public const string fileOutputFileName = "fatec_demanda_data.csv";

        public const int TimeoutShort = 100;
        public const int TimeoutMedium = 200;
        public const int TimeoutLong = 300;

        static void Main(string[] args)
        {
            try
            {
                DownloadDemandsAsync().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex);
            }
            finally
            {
                WebDriver?.Quit();
            }
        }


        public async static Task<bool> DownloadDemandsAsync()
        {
            var totalStopwatch = Stopwatch.StartNew(); // Início do tempo total

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--disable-extensions");

            chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.images", 2);

            WebDriver = new ChromeDriver(chromeOptions);
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(10));

            var selectFatecHelper = new SelectFatecHelper(WebDriver);
            var homeDemandasHelper = new HomeDemandasHelper(WebDriver);

            await homeDemandasHelper.GoToHomePageAsync();

            var acceptedCookies = homeDemandasHelper.ClickOnConsentCookiesButton();
            Console.WriteLine(acceptedCookies ? "Cookie consent accepted." : "Cookie consent element not found or not displayed.");

            var demandDataList = new List<DemandaFatec>();
            var availableSemesters = homeDemandasHelper.GetAllSemesterOptions(true);

            const string targetFatecOriginal = "Mauá";
            var normalizedTargetFatec = NormalizeText(targetFatecOriginal);
            var formattedFatecName = ParseFatecName(targetFatecOriginal);


            foreach (var semesterOption in availableSemesters)
            {
                var semesterStopwatch = Stopwatch.StartNew();

                var semesterSelect = homeDemandasHelper.GetSemesterSelectElement();
                semesterSelect.SelectByValue(semesterOption.value);

                var showButton = wait.Until(d => d.FindElement(By.CssSelector("#formDemanda button.btn[type='send']")));
                showButton.Click();

                wait.Until(d => d.FindElements(By.CssSelector("#FATEC option")).Count > 2);

                var fatecSelect = selectFatecHelper.GetFatecSelectElement();
                var allFatecOptions = fatecSelect.Options
                    .Where(option => !option.Text.ToLower().Contains("selecione"))
                    .ToList();

                var selectedFatecOption = allFatecOptions
                    .FirstOrDefault(option => NormalizeText(option.Text).Contains(normalizedTargetFatec));

                if (selectedFatecOption != null)
                {
                    fatecSelect.SelectByValue(selectedFatecOption.GetAttribute("value"));
                }
                else
                {
                    Console.WriteLine($"Fatec '{targetFatecOriginal}' not found.");
                    await homeDemandasHelper.GoToHomePageAsync();
                    continue;
                }

                var showFatecButton = wait.Until(d => d.FindElement(By.CssSelector("#formDemanda > button")));
                showFatecButton.Click();

                wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
                var tableRows = WebDriver.FindElements(By.CssSelector("table tbody tr"));

                foreach (var row in tableRows)
                {
                    var columns = row.FindElements(By.TagName("td"));
                    if (columns.Count != 5) continue;

                    string courseName = columns[0].Text;
                    string period = columns[1].Text;

                    if (!int.TryParse(columns[2].Text.Trim(), out int applicants)) continue;
                    if (!int.TryParse(columns[3].Text.Trim(), out int seats) || seats == 0) continue;

                    var demandRecord = new DemandaFatec
                    {
                        Fatec = formattedFatecName,
                        Ano = semesterOption.year,
                        Semestre = semesterOption.semester,
                        Curso = CapitalizeText(courseName.Trim()),
                        Periodo = CapitalizeText(period.Trim()),
                        Inscritos = applicants,
                        Vagas = seats,
                        Demanda = applicants / (double)seats
                    };

                    //Console.WriteLine($"Added: {demandRecord}");
                    demandDataList.Add(demandRecord);
                }

                semesterStopwatch.Stop();

                Console.WriteLine($"Levou {FormatElapsedTime(semesterStopwatch.Elapsed)} para salvar as informações da Fatec {formattedFatecName} do {semesterOption.semester}º Semestre de {semesterOption.year}");

                await homeDemandasHelper.GoToHomePageAsync();
            }



            CsvExporterService.SaveToCsv(demandDataList, fileOutputFileName);

            totalStopwatch.Stop(); // Fim do tempo total

            Console.WriteLine($"⏱Tempo total de execução: {FormatElapsedTime(totalStopwatch.Elapsed)}");

            return true;
        }

        public static string CapitalizeText(string input)
        {
            input = input.ToLowerInvariant();
            TextInfo textInfo = CultureInfo.GetCultureInfo("pt-BR").TextInfo;
            return textInfo.ToTitleCase(input);
        }

        public static string ParseFatecName(string input)
        {
            input = input.Trim().ToLowerInvariant();
            TextInfo textInfo = CultureInfo.GetCultureInfo("pt-BR").TextInfo;
            input = textInfo.ToTitleCase(input);

            if (!input.StartsWith("Fatec"))
                input = "Fatec " + input;

            return input;
        }


        public static string NormalizeText(string text)
        {
            if (text == null) return null;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var withoutAccents = new string(normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            return withoutAccents.ToUpperInvariant().Trim();
        }
        public static string FormatElapsedTime(TimeSpan elapsed)
        {
            var parts = new List<string>();

            if (elapsed.Minutes > 0)
                parts.Add($"{elapsed.Minutes}m");

            if (elapsed.Seconds > 0 || elapsed.Minutes > 0)
                parts.Add($"{elapsed.Seconds}s");

            parts.Add($"{elapsed.Milliseconds}ms");

            return string.Join(" ", parts);
        }

    }

}