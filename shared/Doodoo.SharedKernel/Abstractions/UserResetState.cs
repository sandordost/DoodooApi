namespace Doodoo.SharedKernel.Abstractions
{
    /// <summary>A user's last daily/weekly reset timestamps (owned by the Users module).</summary>
    public sealed record UserResetState(DateTime? LastDailyReset, DateTime? LastWeeklyReset);
}
