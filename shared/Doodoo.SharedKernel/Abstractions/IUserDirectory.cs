namespace Doodoo.SharedKernel.Abstractions
{
    /// <summary>
    /// Looks up users by identity fields. Implemented by the host (which owns Identity/AppUser)
    /// so modules can resolve a user without referencing the Users module.
    /// </summary>
    public interface IUserDirectory
    {
        /// <summary>Returns the user id for an email, or null if no such user exists.</summary>
        Task<Guid?> FindUserIdByEmailAsync(string email);
    }
}
