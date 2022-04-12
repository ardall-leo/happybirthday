using HappyBirthday.Domain.Interfaces;
using HappyBirthday.Domain.Models.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace HappyBirthday.API
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IBirthdayWorkflow _birthdayWorkflow;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)//, IBirthdayWorkflow birthdayWorkflow)
        {
            _logger = logger;
            _birthdayWorkflow = serviceProvider.GetService<IBirthdayWorkflow>(); // birthdayWorkflow;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _birthdayWorkflow.ExecuteAsync();

                var next = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(1), DateTimeKind.Utc);
                var sleepingTime = next.Subtract(DateTime.UtcNow);
                await Task.Delay(sleepingTime); 
            }
        }
    }
}
