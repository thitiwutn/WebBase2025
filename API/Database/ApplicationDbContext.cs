using API.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace API.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed default admin user
            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "$2a$11$mVDBrJu5W4Z/GTdlqSAgJOXUlQIXxKMb4LkCN.Ar8OU5h/thf/vqy", // BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Default password
                Role = "Admin"
            };

            modelBuilder.Entity<User>().HasData(adminUser);
        }
    }
}
