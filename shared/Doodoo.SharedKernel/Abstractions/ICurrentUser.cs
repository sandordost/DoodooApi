namespace Doodoo.SharedKernel.Abstractions
{
    /// <summary>
    /// Resolves the authenticated user's id from the current request. Implemented by the host
    /// (which owns the HTTP context) so module controllers can identify the caller without
    /// referencing the host or the Users module.
    /// </summary>
    public interface ICurrentUser
    {
        Guid? GetCurrentUserId();
        Guid GetCurrentUserIdOrThrow();
    }
}
