using HappyBirthday.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyBirthday.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<User> Get()
        {
            var rng = new Random();
            return null;
        }

        [HttpGet]
        public IEnumerable<User> GetBirthday()
        {
            return null;
        }

        [HttpPost]
        public Task Create([FromBody] User newUser)
        {

            return null;
        }

        [HttpDelete]
        public Task Delete(string id)
        {

            return null;
        }
    }
}
