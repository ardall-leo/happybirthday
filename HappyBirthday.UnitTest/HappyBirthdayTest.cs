using Bogus;
using HappyBirthday.Domain.Models;
using HappyBirthday.Infrastructure.Services;
using Moq;
using Moq.AutoMock;
using NodaTime;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HappyBirthday.UnitTest
{
    public class HappyBirthdayTest
    {
        private readonly AutoMocker mocker;

        public HappyBirthdayTest()
        {
            mocker = new AutoMocker();
        }

        /// <summary>
        /// This unit test verify the delayed delivery for greeting to be sent from utcnow to 9am user local time
        /// </summary>
        /// <param name="utcNow"></param>
        /// <param name="birthday"></param>
        /// <param name="location"></param>
        /// <param name="delayDelivery"></param>
        [Theory]
        [InlineData("2022-10-19 00:00", "2019-10-19", "Asia/Bangkok", 120)]
        [InlineData("2022-10-19 02:00", "2019-10-19", "Asia/Bangkok", 0)]
        [InlineData("2022-10-19 02:00", "2019-10-19", "America/New_York", 660)]
        public void GetScheduleForGreeting_Correctly(string utcNow, string birthday, string location, int delayDelivery)
        {
            var faker = new Faker("en");
            var dt = DateTime.Parse(utcNow);
            mocker.GetMock<BirthdayService>().Setup(x => x.Now).Returns(Instant.FromUtc(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute));

            var result = mocker.GetMock<BirthdayService>().Object.GetScheduleForGreeting(new User
            {
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Birthday = DateTime.Parse(birthday),
                Location = location
            });

            Assert.Equal(delayDelivery, result.TotalMinutes);
        }

        /// <summary>
        /// This unit test verify if a greeting will be scheduled or not
        /// </summary>
        /// <param name="utcNow"></param>
        /// <param name="birthday"></param>
        /// <param name="location"></param>
        /// <param name="sent"></param>
        [Theory]
        [InlineData("2022-10-19 00:00", "2019-10-19", "Asia/Bangkok", 1)]
        [InlineData("2022-10-19 02:00", "2019-10-19", "Asia/Bangkok", 1)]
        [InlineData("2022-10-19 02:00", "2019-10-19", "America/New_York", 1)]
        [InlineData("2022-10-20 02:00", "2019-10-19", "America/New_York", 0)] // local time already passed the schedule
        [InlineData("2022-11-20 02:00", "2019-10-19", "America/New_York", 0)] // different month
        public async void SendHappBirthday_Correctly(string utcNow, string birthday, string location, int sent)
        {
            var faker = new Faker("en");
            var dt = DateTime.Parse(utcNow);

            var user = new User
            {
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Birthday = DateTime.Parse(birthday),
                Location = location
            };

            mocker.GetMock<BirthdayService>().Setup(x => x.Now).Returns(Instant.FromUtc(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute));
            mocker.GetMock<BirthdayService>().Setup(x => x.ScheduleGreeting(It.IsAny<User>(), It.IsAny<TimeSpan>())).Returns(Task.FromResult(1));

            await mocker.GetMock<BirthdayService>().Object.SendHappyBirthday(user);
            mocker.GetMock<BirthdayService>().Verify(x => x.ScheduleGreeting(user, It.IsAny<TimeSpan>()), Times.Exactly(sent));
        }
    }
}
