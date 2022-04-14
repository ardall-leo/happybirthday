using Dapper;
using HappyBirthday.Domain;
using HappyBirthday.Domain.Interfaces;
using HappyBirthday.Domain.Models;
using HappyBirthday.Domain.Models.Configs;
using HappyBirthday.Domain.Models.Entities;
using HappyBirthday.Domain.Models.Events;
using NodaTime;
using NServiceBus;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HappyBirthday.Infrastructure.Services
{
    public class BirthdayService : IBirthdayService
    {
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IMessageSession _session;
        private readonly ApiClient _apiClient;
        private readonly AppConfig _config;

        public virtual Instant Now => SystemClock.Instance.GetCurrentInstant();

        public BirthdayService(IDbConnectionFactory dbFactory, IMessageSession session, ApiClient apiClient, AppConfig config)
        {
            _dbFactory = dbFactory;
            _session = session;
            _apiClient = apiClient;
            _config = config;
        }

        public async Task<User> CreateUser(User user)
        {
            using (var connection = _dbFactory.GetConnection(DatabaseType.Hbd))
            {
                var sql = @$"
                    INSERT INTO dbo.[User] (id, FirstName, LastName, Birthday, Location)
                    OUTPUT INSERTED.id, INSERTED.FirstName, INSERTED.LastName, INSERTED.Birthday, INSERTED.Location
                    VALUES ('{Guid.NewGuid()}','{user.FirstName}','{user.LastName}','{user.Birthday}','{user.Location}')
                ";

                return await connection.QuerySingleAsync<User>(sql);
            }
        }

        public async Task<int> DeleteUser(string userId)
        {
            using (var connection = _dbFactory.GetConnection(DatabaseType.Hbd))
            {
                return await connection.ExecuteAsync($"DELETE FROM dbo.[User] WHERE id = '{userId}'");
            }
        }

        public async Task<IEnumerable<User>> GetBirthdayUsers()
        {
            var dateNow = $"{Now.ToDateTimeUtc().ToString("yyyy-MM-dd")}";
            using (var connection = _dbFactory.GetConnection(DatabaseType.Hbd))
            {
                connection.Open();
                string query = @$"
                    SELECT t1.Id, t1.FirstName, t1.LastName, t1.Location, t1.Birthday
                    FROM dbo.[User] t1
                    LEFT JOIN dbo.[BirthdayGreeting] t2
                        ON t1.Id = t2.UserId
                    WHERE (__SCHEDULED_TS is null or __SENT_TS > '{Now.ToDateTimeUtc().ToString("yyyy-MM-dd")}') AND DATEADD(year, (year(getdate()) - year(Birthday)), BirthDay) BETWEEN DATEADD(day, -1, '{dateNow}') AND DATEADD(day, 1, '{dateNow}')
                ";

                return await connection.QueryAsync<User>(query);
            }
        }

        public async Task SendHappyBirthday(User user)
        {
            TimeSpan delayedDelivery = GetScheduleForGreeting(user);

            // makes sure it's within a day
            if (delayedDelivery.TotalMilliseconds >= 0 && delayedDelivery.TotalSeconds < 86400)
            {
                await ScheduleGreeting(user, delayedDelivery).ConfigureAwait(false);
            }
        }

        public virtual async Task ScheduleGreeting(User user, TimeSpan delayedDelivery)
        {
            var option = new SendOptions();
            option.DelayDeliveryWith(delayedDelivery);
            option.RouteToThisEndpoint();
            var greetingId = await this.StoreGreeting(user);
            await _session.Send(new BirthdayEvent { GreetingId = greetingId, BirthdayUser = user }, option).ConfigureAwait(false);
        }

        public TimeSpan GetScheduleForGreeting(User user)
        {
            var userZone = DateTimeZoneProviders.Tzdb[user.Location];
            var schedule = new LocalDateTime(Now.InUtc().Year, user.Birthday.Month, user.Birthday.Day, 9, 00);
            var localSchedule = userZone.AtStrictly(schedule);
            var localTime = Now.InZone(userZone);

            var delayedDelivery = localSchedule.Minus(localTime).ToTimeSpan();
            return delayedDelivery;
        }

        public virtual Task<Guid> StoreGreeting(User user)
        {
            using (var connection = _dbFactory.GetConnection(DatabaseType.Hbd))
            {
                connection.Open();

                string query = @$"
                    INSERT INTO [dbo].[BirthdayGreeting] (Id, UserId,year,__scheduled_ts)
                    OUTPUT INSERTED.id
                    VALUES ('{Guid.NewGuid()}','{user.Id}','{Now.InUtc().Year}', '{Now.InUtc().ToDateTimeUtc()}')
                ";

                return Task.FromResult(connection.QuerySingle<Guid>(query));
            }
        }

        public virtual async Task UpdateGreeting(Guid GreetingId, User user)
        {
            using (var connection = _dbFactory.GetConnection(DatabaseType.Hbd))
            {
                connection.Open();
                string query = @$"
                    UPDATE [dbo].[BirthdayGreeting] SET __sent_ts = '{Now.InUtc().ToDateTimeUtc()}'
                    WHERE Id = '{GreetingId}'
                ";

                await connection.ExecuteAsync(query);
            }
        }

        public async Task SayHappyBirthday(User user)
        {
            var data = $"Hey, {string.Join(" ", user.FirstName, user.LastName)} it's your birthday";
            await _apiClient.SendRequest<UserResponse>(_config.Hookbin, HttpMethod.Post, data);
        }
    }
}
