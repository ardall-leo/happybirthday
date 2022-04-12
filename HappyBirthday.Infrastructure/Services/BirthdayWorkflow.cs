using HappyBirthday.Domain.Interfaces;
using HappyBirthday.Domain.Models.Events;
using NodaTime;
using NodaTime.Extensions;
using NServiceBus;
using System.Threading.Tasks;

namespace HappyBirthday.Infrastructure.Services
{
    public class BirthdayWorkflow : IBirthdayWorkflow
    {
        private readonly IBirthdayService _service;
        private readonly IMessageSession _session;

        public BirthdayWorkflow(IBirthdayService service, IMessageSession session)
        {
            _service = service;
            _session = session;
        }

        public async Task ExecuteAsync()
        {
            var users = _service.GetBirthdayUsers();
            foreach(var user in users)
            {
                var clock = SystemClock.Instance;
                var userZone = DateTimeZoneProviders.Tzdb[user.Location];
                var schedule = new LocalDateTime(clock.InUtc().GetCurrentDate().Year, user.Birthday.Month, user.Birthday.Day, 9, 00);
                var localSchedule = userZone.AtStrictly(schedule);
                var localTime = clock.InZone(userZone).GetCurrentZonedDateTime();

                var delayedDelivery = localSchedule.Minus(localTime).ToTimeSpan();
                
                // makes sure it's within a day
                if (delayedDelivery.TotalMilliseconds > 0 && delayedDelivery.TotalSeconds < 86400)
                {
                    await _service.SendHappyBirthday(user);
                    var option = new SendOptions();
                    option.DelayDeliveryWith(delayedDelivery);
                    option.RouteToThisEndpoint();

                    await _session.Send(new BirthdayEvent { BirthdayUser = user }, option).ConfigureAwait(false);
                }
            }
        }
    }
}
