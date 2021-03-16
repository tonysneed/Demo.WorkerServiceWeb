using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.WorkerServiceWeb
{
    public class Worker : BackgroundService
    {
        private readonly WeatherFactory _factory;
        private readonly WeatherRepository _weatherRepository;
        private readonly ILogger<Worker> _logger;

        public Worker(WeatherFactory factory, WeatherRepository weatherRepository, ILogger<Worker> logger)
        {
            _factory = factory;
            _weatherRepository = weatherRepository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // Create weather and add to repository
                var weather = _factory.CreateWeather();
                _weatherRepository.WeatherForecasts = weather;

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
