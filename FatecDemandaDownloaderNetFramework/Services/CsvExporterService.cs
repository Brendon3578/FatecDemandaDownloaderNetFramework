using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using static FatecDemandaDownloaderNetFramework.Utils;

namespace FatecDemandaDownloaderNetFramework.Services
{
    public static class CsvExporterService
    {
        public static void SaveToCsv(List<DemandaFatec> records, string fileName)
        {
            Log("Saving data to CSV file...");

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Fatec;Ano;Semestre;Curso;Periodo;Inscritos;Vagas;Demanda");

            foreach (var record in records)
            {
                string demandFormatted = record.Demanda.ToString("F2", CultureInfo.InvariantCulture);
                csvBuilder.AppendLine($"{EscapeCsv(record.Fatec)};{record.Ano};{record.Semestre};{EscapeCsv(record.Curso)};{EscapeCsv(record.Periodo)};{record.Inscritos};{record.Vagas};{demandFormatted}");
            }

            File.WriteAllText(fileName, csvBuilder.ToString(), Encoding.UTF8);
            Log($"File '{fileName}' saved successfully!");
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(";") || value.Contains("\""))
            {
                value = value.Replace("\"", "\"\"");
                value = $"\"{value}\"";
            }
            return value;
        }
    }
}
