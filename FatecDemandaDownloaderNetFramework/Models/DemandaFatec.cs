using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatecDemandaDownloaderNetFramework
{
    public class DemandaFatec
    {
        public string Fatec { get; set; }
        public int Ano { get; set; }
        public int Semestre { get; set; }
        public string Curso { get; set; }
        public string Periodo { get; set; }
        public int Inscritos { get; set; }
        public int Vagas { get; set; }
        public double Demanda { get; set; }

        public override string ToString()
        {
            return $"Fatec: {Fatec}, Ano: {Ano}, Semestre: {Semestre}, Curso: {Curso}, Periodo: {Periodo}, " +
                   $"Inscritos: {Inscritos}, Vagas: {Vagas}, Demanda: {Demanda.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}";
        }
    }
}
