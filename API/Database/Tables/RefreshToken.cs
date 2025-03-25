namespace API.Database.Tables
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;

        public User? User { get; set; }
    }
}
