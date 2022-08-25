using AuthWebAPI.Data;
using AuthWebAPI.DTOs;
using AuthWebAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace AuthWebAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _dbContext;

        public AuthService(IConfiguration configuration, DataContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<User> Register(UserRegisterDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public bool CheckRigistrationPassword(string password, string confirmPassword)
        {
            Regex passwordRegex = new Regex(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%_-]).{8,12}");

            if (!passwordRegex.IsMatch(password) || !passwordRegex.IsMatch(confirmPassword))
            {
                return false;
            }
            else if (password != confirmPassword)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public string CreateJwtToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }

        public RefreshToken GenerateRefreshJwtToken()
        {
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            return refreshToken;
        }

        public async Task SetRefreshToken(User user, RefreshToken newRefreshToken, HttpResponse response)
        {
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            user.RefreshTokens.Add(newRefreshToken);
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ActionResult<string>> RefreshToken(HttpRequest request, HttpResponse response)
        {
            var refreshToken = request.Cookies["refreshToken"];
            var userRefreshToken = _dbContext.Users.SelectMany(u => u.RefreshTokens).FirstOrDefault(t => t.Token == refreshToken);
            var user = _dbContext.Users.Where(u => u.RefreshTokens.Contains(userRefreshToken)).FirstOrDefault();

            if (user != null)
            {
                if (userRefreshToken == null || !(userRefreshToken.Token).Equals(refreshToken))
                {
                    return new UnauthorizedObjectResult("Invalid Refresh Token");
                }
                else if (userRefreshToken.IsExpired)
                {
                    return new UnauthorizedObjectResult("Token expired");
                }

                string newToken = CreateJwtToken(user);
                var newRefreshToken = GenerateRefreshJwtToken();
                await SetRefreshToken(user, newRefreshToken, response);

                return new OkObjectResult(newToken);
            }
            else
            {
                return new BadRequestObjectResult("User not found");
            }
        }


    }
}
