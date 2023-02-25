﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using WebApplication8.Data;

namespace WebApplication8;


public class CustomUserStore<TUser, TContext, TKey> : IUserStore<TUser>, IUserEmailStore<TUser>, IUserPasswordStore<TUser>, IUserSecurityStampStore<TUser>
    where TUser : class, ICustomIdentityUser<TKey>
    where TContext : DbContext, IWithUsersDbContext<TUser>
    where TKey : IEquatable<TKey>
{

    //ILookupNormalizer
    public CustomUserStore(TContext context, IdentityErrorDescriber describer = null)
    {
        Context = context;
        ErrorDescriber = describer;
    }

    private bool _disposed;

    /// <summary>
    /// Gets or sets the <see cref="IdentityErrorDescriber"/> for any error that occurred with the current operation.
    /// </summary>
    public IdentityErrorDescriber ErrorDescriber { get; set; }

    /// <summary>
    /// Gets the database context for this store.
    /// </summary>
    public virtual TContext Context { get; private set; }

    private DbSet<TUser> UsersSet { get { return Context.Set<TUser>(); } }

    /// <summary>
    /// Gets or sets a flag indicating if changes should be persisted after CreateAsync, UpdateAsync and DeleteAsync are called.
    /// </summary>
    /// <value>
    /// True if changes should be automatically persisted, otherwise false.
    /// </value>
    public bool AutoSaveChanges { get; set; } = true;

    /// <summary>Saves the current store.</summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    protected Task SaveChanges(CancellationToken cancellationToken)
    {
        return AutoSaveChanges ? Context.SaveChangesAsync(cancellationToken) : Task.CompletedTask;
    }

    /// <summary>
    /// Creates the specified <paramref name="user"/> in the user store.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the creation operation.</returns>
    public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        Context.Add(user);
        await SaveChanges(cancellationToken);
        return IdentityResult.Success;
    }

    /// <summary>
    /// Updates the specified <paramref name="user"/> in the user store.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
    public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        Context.Attach(user);
        // RIP user.ConcurrencyStamp = Guid.NewGuid().ToString();
        Context.Update(user);
        try
        {
            await SaveChanges(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
        return IdentityResult.Success;
    }

    /// <summary>
    /// Deletes the specified <paramref name="user"/> from the user store.
    /// </summary>
    /// <param name="user">The user to delete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
    public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        Context.Remove(user);
        try
        {
            await SaveChanges(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
        return IdentityResult.Success;
    }

    /// <summary>
    /// Finds and returns a user, if any, who has the specified <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user ID to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="userId"/> if it exists.
    /// </returns>
    public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var id = ConvertIdFromString(userId);
        return UsersSet.FindAsync(new object[] { id }, cancellationToken).AsTask();
    }

    /// <summary>
    /// Finds and returns a user, if any, who has the specified normalized user name.
    /// </summary>
    /// <param name="normalizedUserName">The normalized user name to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="normalizedUserName"/> if it exists.
    /// </returns>
    public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    /// <summary>
    /// Converts the provided <paramref name="id"/> to a strongly typed key object.
    /// </summary>
    /// <param name="id">The id to convert.</param>
    /// <returns>An instance of <typeparamref name="TKey"/> representing the provided <paramref name="id"/>.</returns>
    public virtual TKey ConvertIdFromString(string id)
    {
        if (id == null)
        {
            return default(TKey);
        }
        return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
    }

    /// <summary>
    /// Converts the provided <paramref name="id"/> to its string representation.
    /// </summary>
    /// <param name="id">The id to convert.</param>
    /// <returns>An <see cref="string"/> representation of the provided <paramref name="id"/>.</returns>
    public virtual string ConvertIdToString(TKey id)
    {
        if (object.Equals(id, default(TKey)))
        {
            return null;
        }
        return id.ToString();
    }

    /// <summary>
    /// A navigation property for the users the store contains.
    /// </summary>
    public IQueryable<TUser> Users => UsersSet;

    public Task<String> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<String> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(ConvertIdToString(user.Id));
    }

    public Task<String> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(TUser user, String normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;

        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(TUser user, String userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Throws if this class has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    /// Dispose the store
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
    }

    public Task SetEmailAsync(TUser user, String email, CancellationToken cancellationToken)
    {
        user.Email = email;

        return Task.CompletedTask;
    }

    public Task<String> GetEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<Boolean> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.IsEmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(TUser user, Boolean confirmed, CancellationToken cancellationToken)
    {
        user.IsEmailConfirmed = confirmed;

        return Task.CompletedTask;
    }

    public Task<TUser> FindByEmailAsync(String normalizedEmail, CancellationToken cancellationToken)
    {
        return Context.GetByEmail(normalizedEmail).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<String> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(TUser user, String normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;

        return Task.CompletedTask;
    }

    // IUserPasswordStore

    public Task SetPasswordHashAsync(TUser user, String passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;

        return Task.CompletedTask;
    }

    public Task<String> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<Boolean> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash != null);
    }

    // IUserSecurityStampStore

    public Task SetSecurityStampAsync(TUser user, String stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;

        return Task.CompletedTask;
    }

    public Task<String> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }
}