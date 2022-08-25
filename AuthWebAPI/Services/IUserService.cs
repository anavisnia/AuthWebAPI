using AuthWebAPI.Entities;

namespace AuthWebAPI.Services
{
    public interface IUserService
    {
        bool CheckIfUserExists(string username);
        Task<User?> GetUser(string username);
        Task<List<User>?> GetUsers();
        Task<List<string>?> GetUsernames();
    }
}
