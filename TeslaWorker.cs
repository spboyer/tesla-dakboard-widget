using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TeslaWidget.Hubs;

namespace TeslaWidget
{
    public class TeslaWorker : BackgroundService
    {
        private ILogger<TeslaWorker> _logger;
        private ITeslaService _service;
        private readonly IConfiguration _configuration;
        private IHubContext<StatusHub, IStatus> _hub;

        public TeslaWorker(ILogger<TeslaWorker> logger, 
                            IConfiguration configuration,
                           ITeslaService service, IHubContext<StatusHub, IStatus> hub)
        {
            _service = service;
            _logger = logger;
            _configuration = configuration;
            _hub = hub;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                SendCarStatus();
                
                if (Debugger.IsAttached)
                {
                    await Task.Delay(10000, stoppingToken);
                } else
                {
                    // 15 minutes
                    var delay = _configuration.GetValue<int>("TIMER_SETTING");
                    await Task.Delay(delay, stoppingToken);
                }
            }
        }

        void SendCarStatus()
        {
            var summary = _service.CarSummary().GetAwaiter().GetResult();
            _hub.Clients.All.SendStatus(summary);
        }
    }
}
