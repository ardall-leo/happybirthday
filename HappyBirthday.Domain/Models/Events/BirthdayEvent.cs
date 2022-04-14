using NServiceBus;
using System;

namespace HappyBirthday.Domain.Models.Events
{
    public class BirthdayEvent : ICommand
    {
        public Guid GreetingId { get; set; }

        public User BirthdayUser { get; set; }
    }
}
