using AuthWebAPI.Entities;
using AuthWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("getuser")]
        public async Task<ActionResult<User?>> GetUser()
        {
            return Ok(await _userService.GetUser(this.User.Identity.Name));
        }

        [HttpGet("getusernames")]
        public async Task<ActionResult<List<string>?>> GetUsernames()
        {
            var users = await _userService.GetUsernames();

            if (users == null)
            {
                return BadRequest("Users not found");
            }

            return Ok(users);
        }

    }
}
