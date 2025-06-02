using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FatecDemandaDownloaderNetFramework
{
    public class Utils
    {
        public static void Log(object message)
        {
            Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {message}");
        }

        public static void Log(object message, bool withBreakLine, bool withTimestamp)
        {
            var parsedMessage = withTimestamp
                ? $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {message}"
                : message.ToString();

            if (withBreakLine)
                Console.WriteLine(parsedMessage);
            else
                Console.Write(parsedMessage);
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
