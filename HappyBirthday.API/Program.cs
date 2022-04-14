using HappyBirthday.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using NServiceBus;
using HappyBirthday.Domain.Interfaces;
using HappyBirthday.Infrastructure.Services;
using HappyBirthday.Domain.Models.Configs;
using HappyBirthday.Infrastructure.Database;

try
{
    using IHost host = Host.CreateDefaultBuilder(args)
        .UseNServiceBus(hostBuilderContext =>
        {
            var config = hostBuilderContext.Configuration;
            var rootConfig = new AppConfig();
            config.Bind("AppConfig", rootConfig);

            var endpointConfiguration = new EndpointConfiguration(rootConfig.ServiceBus.EndpointName);

            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(rootConfig.ServiceBus.ConnStr);

            endpointConfiguration.EnableInstallers();

            // configure retry mechanism
            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Immediate(i => i.NumberOfRetries(0));    // disabling immediate retry
            recoverability.Delayed(d =>
            {
                d.NumberOfRetries(rootConfig.ServiceBus.MaxRetryAttempt);
                d.TimeIncrease(TimeSpan.FromMinutes(rootConfig.ServiceBus.RetryInterval));
            });

            // when the error persist, move to retry queue, this needs to involve administrator to check it further why the error persist
            // and use ServiceInsight to retry
            endpointConfiguration.SendFailedMessagesTo(rootConfig.ServiceBus.RetryEndpoint);

            return endpointConfiguration;
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .ConfigureServices((hostContext, services) =>
        {
            var config = hostContext.Configuration;
            var rootConfig = new AppConfig();
            config.Bind("AppConfig", rootConfig);
            services.AddSingleton(rootConfig);
            services.AddLogging(s => s.AddSerilog());
            services.AddSingleton<ApiClient>();
            services.AddSingleton<IDbConnectionFactory, DbFactory>();
            services.AddTransient<IBirthdayService, BirthdayService>();
            services.AddSingleton<IBirthdayWorkflow, BirthdayWorkflow>();
            services.AddHostedService<Worker>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    throw;
}
