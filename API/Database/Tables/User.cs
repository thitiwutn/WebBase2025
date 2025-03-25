namespace API.Database.Tables
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
