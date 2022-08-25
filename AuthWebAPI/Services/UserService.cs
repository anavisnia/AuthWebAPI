using AuthWebAPI.Data;
using AuthWebAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthWebAPI.Services
{
    public class UserService : IUserService
    {
        protected readonly DataContext _dbContext;

        public UserService(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool CheckIfUserExists(string username)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<User?> GetUser(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<List<User>?> GetUsers()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task<List<string>?> GetUsernames()
        {
            return await _dbContext.Users.Select(u => u.Username).ToListAsync();
        }


    }
}
