using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static FatecDemandaDownloaderNetFramework.Utils;

namespace UnitTests
{
    [TestClass]
    public class UtilsClassTest
    {
        [DataTestMethod]
        [DataRow("engenharia de produção", "Engenharia De Produção")]
        [DataRow("ADMINISTRAÇÃO", "Administração")]
        [DataRow("sistemas para internet", "Sistemas Para Internet")]
        public void CapitalizeText_ShouldReturnCorrectTitleCase(string input, string expected)
        {
            var result = CapitalizeText(input);
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow("mauá", "Fatec Mauá")]
        [DataRow(" FATEC Zona Leste ", "Fatec Zona Leste")]
        [DataRow("fatec santos", "Fatec Santos")]
        public void ParseFatecName_ShouldFormatProperly(string input, string expected)
        {
            var result = ParseFatecName(input);
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow("São Paulo", "SAO PAULO")]
        [DataRow("Fatec São Caetano", "FATEC SAO CAETANO")]
        [DataRow("Zona Leste", "ZONA LESTE")]
        public void NormalizeText_ShouldRemoveAccentsAndToUpper(string input, string expected)
        {
            var result = NormalizeText(input);
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow(0, 0, 300, "300ms")]
        [DataRow(0, 5, 100, "5s 100ms")]
        [DataRow(2, 30, 50, "2m 30s 50ms")]
        public void FormatElapsedTime_ShouldReturnFormattedTime(int minutes, int seconds, int milliseconds, string expected)
        {
            var time = new TimeSpan(0, 0, minutes, seconds, milliseconds);
            var result = FormatElapsedTime(time);
            Assert.AreEqual(expected, result);
        }
    }
}
