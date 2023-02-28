using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Data
{
    public class DummyUser : Microsoft.AspNetCore.Identity.IdentityUser { }

    public class DummyContext : IdentityDbContext { }

    public interface ICustomIdentityUser
    {
        String Email { get; set; }

        String NormalizedEmail { get; set; }

        String UserName { get; set; }

        String NormalizedUserName { get; set; }

        Boolean IsEmailConfirmed { get; set; }

        String PasswordHash { get; set; }

        String SecurityStamp { get; set; }
    }

    public interface ICustomIdentityUser<TKey> : ICustomIdentityUser
    {
        TKey Id { get; set; }
    }

    public interface IWithUsersDbContext<TUser>
    {
        IQueryable<TUser> GetByEmail(String normalizedEmail);
    }

    public class User : ICustomIdentityUser<Guid>
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(120)]
        public String Email { get; set; } = "";

        [Required]
        [StringLength(120)]
        public String NormalizedEmail { get; set; } = "";

        [Required]
        [StringLength(120)]
        public String UserName { get; set; } = "";

        [Required]
        [StringLength(120)]
        public String NormalizedUserName { get; set; } = "";

        public Boolean IsEmailConfirmed { get; set; }

        [StringLength(120)]
        public String PasswordHash { get; set; }

        [StringLength(120)]
        public String SecurityStamp { get; set; }
    }

    public class ApplicationDbContext : DbContext, IWithUsersDbContext<User>
    {
        public DbSet<User> Users { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public IQueryable<AppUser> GetByEmail(String normalizedEmail)
        {
            return Users.Where(u => u.NormalizedEmail == normalizedEmail);
        }
    }
}