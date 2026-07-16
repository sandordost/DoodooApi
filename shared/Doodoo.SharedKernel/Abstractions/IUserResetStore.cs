namespace Doodoo.SharedKernel.Abstractions
{
    /// <summary>
    /// Reads/writes a user's daily/weekly reset markers. Implemented by the host so the Todos
    /// module can drive its recurring-item resets without referencing the Users module or AppUser.
    /// </summary>
    public interface IUserResetStore
    {
        Task<UserResetState?> GetAsync(Guid userId);
        Task SetAsync(Guid userId, DateTime? lastDailyReset, DateTime? lastWeeklyReset);
    }
}
