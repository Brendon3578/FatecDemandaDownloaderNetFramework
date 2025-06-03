using FatecDemandaDownloaderNetFramework.Services;
using System;
using System.Threading.Tasks;
using static FatecDemandaDownloaderNetFramework.Utils;

namespace FatecDemandaDownloaderNetFramework
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var demandDownloaderService = new DemandDownloaderService();
                await demandDownloaderService.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Log($"Unexpected error: {ex.Message}");
                Log(ex);
            }
        }
    }
}