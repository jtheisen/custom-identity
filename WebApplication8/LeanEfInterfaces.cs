namespace WebApplication8;

public interface ILeanEfIdentityUser
{
    String Email { get; set; }

    String NormalizedEmail { get; set; }

    String UserName { get; set; }

    String NormalizedUserName { get; set; }

    Boolean IsEmailConfirmed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    String PasswordHash { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    String SecurityStamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

public interface ILeanEfIdentityUser<TKey> : ILeanEfIdentityUser
{
    TKey Id { get; set; }
}

public interface ILeanEfIdentityDbContext<TUser, TKey>
{
    IQueryable<TUser> GetUsers();

    IQueryable<TUser> GetUsersById(TKey id);

    IQueryable<TUser> GetUsersByName(String normalizedName);

    IQueryable<TUser> GetUsersByEmail(String normalizedEmail) => throw new NotImplementedException();
}
