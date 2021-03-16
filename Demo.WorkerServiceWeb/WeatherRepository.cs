using System.Collections.Generic;

namespace Demo.WorkerServiceWeb
{
    public class WeatherRepository
    {
        public List<WeatherForecast> WeatherForecasts { get; set; } = new List<WeatherForecast>();
    }
}
