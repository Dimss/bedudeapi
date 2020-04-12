using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BeDudeApi.Models;
using BeDudeApi.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace BeDudeApi
{
    public class TimedLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public TimedLogger(ILogger logger) => _logger = logger;

        public TimedLogger(ILoggerFactory loggerFactory) : this(new Logger<T>(loggerFactory)) { }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            _logger.Log(logLevel, eventId, state, exception, (s, ex) => $"[{DateTime.UtcNow:HH:mm:ss.fff}]: {formatter(s, ex)}");

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
    }

    public class Startup
    {

        readonly string AllowCORS = "_allowCORS";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CoronaStatusDatabaseSettings>(Configuration.GetSection(nameof(CoronaStatusDatabaseSettings)));

            services.AddSingleton<ICoronaStatusDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CoronaStatusDatabaseSettings>>().Value);

            services.AddHostedService<DataLoaderBGService>();

            services.AddCors(options => { options.AddPolicy(name: AllowCORS, builder => { builder.WithOrigins("*"); }); });

            // Custom loggin hack ?!
            services.Add(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));



            services.AddSingleton<CoronaStatusService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(AllowCORS);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
