using System.Text.Json.Serialization;

namespace AuthWebAPI.Entities
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Username { get; set; }
        [JsonIgnore]
        public byte[] PasswordHash { get; set; }
        [JsonIgnore]
        public byte[] PasswordSalt { get; set; }
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
