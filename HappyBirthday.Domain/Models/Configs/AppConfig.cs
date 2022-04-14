using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyBirthday.Domain.Models.Configs
{
    public record AppConfig
    {
        public string DbConnStr { get; init; }

        public string Hookbin { get; init; }

        public bool UseMigration { get; init; }

        public ServiceBus ServiceBus { get; init; }
    }

    public record ServiceBus
    {
        public string EndpointName { get; init; }

        public string ConnStr { get; init; }

        public string RetryEndpoint { get; init; }

        public int MaxRetryAttempt { get; init; }

        public int RetryInterval { get; init; }
    }
}
