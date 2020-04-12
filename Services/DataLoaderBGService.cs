using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace BeDudeApi.Services
{

    internal class DataLoaderBGService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly CoronaStatusService _coronaStatusService;
        private Timer _timer;

        public DataLoaderBGService(ILogger<DataLoaderBGService> logger, CoronaStatusService coronaStatusService)
        {
            _logger = logger;
            _coronaStatusService = coronaStatusService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(" ##### INIT BG TASK ##### ");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation(" ##### RUNNING BG TASK ##### ");
            _logger.LogInformation("Fetching CoronaStatus data.");
            _coronaStatusService.LoadData();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CoronaStatus Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}