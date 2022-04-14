using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyBirthday.Domain.Models.Entities
{
    public record UserResponse
    {
        public bool Success { get; init; }
    }
}
