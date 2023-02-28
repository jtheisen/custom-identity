using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Data
{
    //public class DummyUser : Microsoft.AspNetCore.Identity.IdentityUser { }

    //public class DummyContext : IdentityDbContext { }

    public interface ILeanEfIdentityUser
    {
        String Email { get; set; }

        String NormalizedEmail { get; set; }

        String UserName { get; set; }

        String NormalizedUserName { get; set; }

        Boolean IsEmailConfirmed { get; set; }

        String PasswordHash { get; set; }

        String SecurityStamp { get; set; }
    }

    public interface ILeanEfIdentityUser<TKey> : ILeanEfIdentityUser
    {
        TKey Id { get; set; }
    }

    public interface IWithUsersDbContext<TUser, TKey>
    {
        IQueryable<TUser> GetUsers();

        IQueryable<TUser> GetUsersById(TKey id);

        IQueryable<TUser> GetUsersByEmail(String normalizedEmail);

        IQueryable<TUser> GetUsersByName(String normalizedName);
    }

    public class User : ILeanEfIdentityUser<Guid>
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(256)]
        public String Email { get; set; } = "";

        [Required]
        [StringLength(256)]
        public String UserName { get; set; } = "";

        public UserPrivateSection PrivateSection { get; set; }

        UserPrivateSection GetPrivateSection() => PrivateSection ?? (PrivateSection = new UserPrivateSection { UserId = Id });

        String ILeanEfIdentityUser.NormalizedEmail { get => Email; set => Email = value; }
        String ILeanEfIdentityUser.NormalizedUserName { get => UserName; set => UserName = value; }
        Boolean ILeanEfIdentityUser.IsEmailConfirmed { get => GetPrivateSection().IsEmailConfirmed; set => GetPrivateSection().IsEmailConfirmed = value; }
        String ILeanEfIdentityUser.PasswordHash { get => GetPrivateSection().PasswordHash; set => GetPrivateSection().PasswordHash = value; }
        String ILeanEfIdentityUser.SecurityStamp { get => GetPrivateSection().SecurityStamp; set => GetPrivateSection().SecurityStamp = value; }
    }

    public class UserPrivateSection
    {
        [Key]
        public Guid UserId { get; set; }

        public AppUser User { get; set; }

        public Boolean IsEmailConfirmed { get; set; }

        [StringLength(120)]
        public String PasswordHash { get; set; }

        [StringLength(36)]
        public String SecurityStamp { get; set; }
    }

    public class ApplicationDbContext : DbContext, IWithUsersDbContext<User, Guid>
    {
        public DbSet<User> Users { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().Property(u => u.Email)
                .UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

            modelBuilder.Entity<User>().Property(u => u.UserName)
                .UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

            modelBuilder.Entity<UserPrivateSection>()
                .HasOne(p => p.User)
                .WithOne(u => u.PrivateSection);
        }

        public IQueryable<AppUser> GetUsers() => Users.Include(u => u.PrivateSection);
        public IQueryable<AppUser> GetUsersById(Guid id) => GetUsers().Where(u => u.Id == id);
        public IQueryable<AppUser> GetUsersByEmail(String normalizedEmail) => GetUsers().Where(u => u.Email == normalizedEmail);
        public IQueryable<AppUser> GetUsersByName(String normalizedName) => GetUsers().Where(u => u.UserName == normalizedName);
    }
}