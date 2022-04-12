using System;

namespace HappyBirthday.Domain.Models
{
    public record User
    {
        public Guid Id { get; init; }

        public string FirstName { get; init; }

        public string LastName { get; init; }

        public DateTime Birthday { get; init; }

        public string Location { get; init; }
    }
}
