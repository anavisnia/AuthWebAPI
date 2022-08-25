using AuthWebAPI.DTOs;
using AuthWebAPI.Entities;
using AuthWebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto? request)
        {
            if (request != null)
            {
                var doesUserExists = _userService.CheckIfUserExists(request.Username);

                if (!doesUserExists)
                {
                    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
                    {
                        return BadRequest("One or more fields are empty or whitespaces");
                    }
                    else if (!_authService.CheckRigistrationPassword(request.Password, request.ConfirmPassword))
                    {
                        return BadRequest("Password and Confirm password differs");
                    }
                    else
                    {
                        return Ok(await _authService.Register(request));
                    }
                }
                else
                {
                    return BadRequest($"User with such username:{request.Username} already exists");
                }
            }
            else
            {
                return BadRequest("Form cannot be empty");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto request)
        {
            if (!_userService.CheckIfUserExists(request.Username))
            {
                return BadRequest("User not found");
            }
            else
            {
                var user = await _userService.GetUser(request.Username);

                if (!_authService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return BadRequest("Wrong password");
                }

                string jwtToken = _authService.CreateJwtToken(user);

                RefreshToken refreshToken = _authService.GenerateRefreshJwtToken();
                await _authService.SetRefreshToken(user, refreshToken, Response);

                return Ok(jwtToken);
            }
        }

        [HttpPost("refreshToken")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            return await _authService.RefreshToken(Request, Response);
        }

    }
}
