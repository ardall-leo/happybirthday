using HappyBirthday.Domain.Models;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyBirthday.Domain.Interfaces
{
    public interface IBirthdayService
    {
        Instant Now { get; }

        Task<IEnumerable<User>> GetBirthdayUsers();

        Task<Guid> StoreGreeting(User user);

        Task UpdateGreeting(Guid GreetingId, User BirthdayUser);

        Task<User> CreateUser(User newUser);

        Task<int> DeleteUser(string userId);

        Task SendHappyBirthday(User user);

        Task SayHappyBirthday(User user);

        Task ScheduleGreeting(User user, TimeSpan delayedDelivery);

        TimeSpan GetScheduleForGreeting(User user);
    }
}
