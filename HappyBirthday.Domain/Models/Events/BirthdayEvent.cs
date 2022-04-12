using NServiceBus;

namespace HappyBirthday.Domain.Models.Events
{
    public class BirthdayEvent : ICommand
    {
        public User BirthdayUser { get; set; }
    }
}
