# Worker Service Web Demo

Demonstrates how to add a Web API to a worder service project.

1. Create a new worker service project.
2. Edit the .csproj file to change the SDK from `Worker` to `Web`.
    ```xml
    <Project Sdk="Microsoft.NET.Sdk.Web">
    ```
3. Add a `WeatherForecast` class.
    ```csharp
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string Summary { get; set; }
    }
    ```
4. Add a `WeatherRepository` class.
    ```csharp
    public class WeatherRepository
    {
        public List<WeatherForecast> WeatherForecasts { get; set; } = new List<WeatherForecast>();
    }
    ```
5. Add a `WeatherFactory` class.
    ```csharp
    public class WeatherFactory
    {
        private readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public List<WeatherForecast> CreateWeather()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToList();
        }
    }
    ```
6. Inject `WeatherFactory` and `WeatherRespository` into the `Worker` class.
   - Then in `ExecuteAsync` use the factor to create weather and add to the weather repository.
   - Increase the delay to 5 seconds.
    ```csharp
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
    ```
7. Add a `Startup` class.
   - Add package: **Swashbuckle.AspNetCore**
    ```csharp
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication2", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication2 v1"));
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    ```
8. Add a `WeatherForecastController` to a `Controllers` folder.
    - Inject `WeatherRepository` to return weather forecasts from a `Get` method.
    ```csharp
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly WeatherRepository _weatherRepo;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(WeatherRepository weatherRepo, ILogger<WeatherForecastController> logger)
        {
            _weatherRepo = weatherRepo;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation($"Weatherforecast requested.");
            return _weatherRepo.WeatherForecasts;
        }
    }
    ```
10. Lastly, in `Program.CreateHostBuilder` register `WeatherRespository` and configure the web host to use the `Startup` class.
    ```csharp
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
                // Register weather factory and repo
                services.AddSingleton<WeatherFactory>();
                services.AddSingleton<WeatherRepository>();
            })
            // Configure web host
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    ```
11. Browse to http://localhost:5000/weatherforecast and refresh the page every few seconds.
    - You should see the values changed by the worker service.