using HappyBirthday.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HappyBirthday.Infrastructure.Services
{
    public class BirthdayWorkflow : IBirthdayWorkflow
    {
        private readonly IBirthdayService _service;
        private readonly ILogger<BirthdayWorkflow> _logger;

        public BirthdayWorkflow(ILogger<BirthdayWorkflow> logger, IBirthdayService service)
        {
            _service = service;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var users = await _service.GetBirthdayUsers();
            foreach(var user in users)
            {
                try
                {
                    await _service.SendHappyBirthday(user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString(), ex);
                }
            }
        }
    }
}
