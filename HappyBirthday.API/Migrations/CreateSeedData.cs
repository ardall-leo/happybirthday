using FluentMigrator;
using FluentMigrator.SqlServer;
using HappyBirthday.Domain.Models;
using NodaTime;
using NodaTime.TimeZones;
using System;
using System.Linq;

namespace HappyBirthday.API.Migrations
{
    [Migration(2022041101)]
    public class InitialSeed_2022041101 : Migration
    {
        public override void Down()
        {
            Delete.FromTable("User").AllRows();
        }

        public override void Up()
        {
            var i = TzdbDateTimeZoneSource.Default;
            var persons = Enumerable.Range(1, 1000)
                .Select(_ => new Bogus.Person())
                .Select(p => new User 
                {
                    Id = Guid.NewGuid(),
                    Birthday = p.DateOfBirth,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Location = i.ZoneLocations[new Random().Next(0, i.ZoneLocations.Count -1)].ZoneId 
                });

            foreach(var person in persons)
            {
                Insert.IntoTable("User").Row(person);
            }
        }
    }
}
