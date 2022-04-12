using HappyBirthday.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using HappyBirthday.Domain.Interfaces;
using HappyBirthday.Infrastructure.Services;

try
{
    using IHost host = Host.CreateDefaultBuilder(args)
        .UseNServiceBus(hostBuilderContext =>
        {
            var endpointConfiguration = new EndpointConfiguration("sender");
            //var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            var connection = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=happybirthday;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            
            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(connection);

            endpointConfiguration.EnableInstallers();

            // configure retry mechanism
            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Immediate(i => i.NumberOfRetries(0));    // disabling immediate retry
            recoverability.Delayed(d =>
            {
                d.NumberOfRetries(3);
                d.TimeIncrease(TimeSpan.FromMinutes(2));
            });

            // when the error persist, move to retry queue, this needs to involve administrator to check it further why the error persist
            // and use ServiceInsight to retry
            endpointConfiguration.SendFailedMessagesTo("retry");

            return endpointConfiguration;
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddLogging(s => s.AddSerilog());
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
