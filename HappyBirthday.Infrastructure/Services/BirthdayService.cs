using Dapper;
using HappyBirthday.Domain.Interfaces;
using HappyBirthday.Domain.Models;
using NodaTime;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;

namespace HappyBirthday.Infrastructure.Services
{
    public class BirthdayService : IBirthdayService
    {
        IDbConnection connection;

        public BirthdayService()
        {
            connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        public IEnumerable<User> GetBirthdayUsers()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var dateNow = $"{now.InUtc().Year}-{now.InUtc().Month}-{now.InUtc().Day}";

            string query = @$"
                SELECT *
                FROM dbo.[User]
                WHERE DATEADD(year, (year(getdate()) - year(Birthday)), BirthDay) BETWEEN DATEADD(day, -1, '{dateNow}') AND DATEADD(day, 1, '{dateNow}')
            ";

            return connection.Query<User>(query);
        }

        public Task SendHappyBirthday(User user)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            string query = @$"
                INSERT INTO [dbo].[BirthdayGreeting] (UserId,year,__sent_ts) VALUES ('{user.Id}','{now.InUtc().Year}','{now.InUtc().ToDateTimeUtc()}')
            ";


            return connection.ExecuteAsync(query);
        }
    }
}
