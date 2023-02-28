using Microsoft.AspNetCore.Identity;
using WebApplication8.Data;

namespace WebApplication8;

public class CustomUserStore : LeanEfStandardWithSecurityStampUserStore<User, ApplicationDbContext, Guid>
{
    public CustomUserStore(ApplicationDbContext context, IdentityErrorDescriber describer) : base(context, describer) { }
}
