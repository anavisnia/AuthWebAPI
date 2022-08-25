using AuthWebAPI.DTOs;
using AuthWebAPI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebAPI.Services
{
    public interface IAuthService
    {
        Task<User> Register(UserRegisterDto request);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool CheckRigistrationPassword(string password, string confirmPassword);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        string CreateJwtToken(User user);
        RefreshToken GenerateRefreshJwtToken();
        Task SetRefreshToken(User user, RefreshToken newRefreshToken, HttpResponse response);
        Task<ActionResult<string>> RefreshToken(HttpRequest request, HttpResponse response);
    }
}
