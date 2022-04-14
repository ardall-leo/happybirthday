using FluentMigrator;
using FluentMigrator.SqlServer;

namespace HappyBirthday.API.Migrations
{
    [Migration(2022041100)]
    public class Migration_User_20220411001 : Migration
    {
        public override void Down()
        {
            Delete.Table("BirthdayGreeting");
            Delete.Table("User");
        }

        public override void Up() 
        {
            Create.Table("User")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("FirstName").AsString().NotNullable()
                .WithColumn("LastName").AsString().NotNullable()
                .WithColumn("Birthday").AsDate().NotNullable()
                .WithColumn("Location").AsString().Nullable();

            Create.Table("BirthdayGreeting")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("UserId").AsGuid().NotNullable().ForeignKey("User", "Id")
                .WithColumn("Year").AsInt32().NotNullable()
                .WithColumn("__SCHEDULED_TS").AsDateTime2().Nullable()
                .WithColumn("__SENT_TS").AsDateTime2().Nullable();

            Create.ForeignKey("FK_BirthdayGreeting_UserId")
               .FromTable("BirthdayGreeting").ForeignColumn("UserId")
               .ToTable("User").PrimaryColumn("Id")
               .OnDeleteOrUpdate(System.Data.Rule.Cascade);
        }
    }
}
