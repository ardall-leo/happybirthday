using HappyBirthday.Domain.Interfaces;
using HappyBirthday.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HappyBirthday.API.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IBirthdayService _birthdayService;

        public UserController(ILogger<UserController> logger, IBirthdayService birthdayService)
        {
            _logger = logger;
            _birthdayService = birthdayService;
        }

        [HttpGet("users/birthday")]
        public async Task<IActionResult> GetBirthday()
        {
            try
            {
                return Ok(await _birthdayService.GetBirthdayUsers());
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("user")]
        public async Task<IActionResult> Create([FromBody] User newUser)
        {
            try
            {
                return Ok(await _birthdayService.CreateUser(newUser));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpDelete("user/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _birthdayService.DeleteUser(id);
                return NoContent();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
