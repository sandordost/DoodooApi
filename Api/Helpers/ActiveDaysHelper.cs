using DoodooApi.Models.Enums.Flags;

namespace DoodooApi.Helpers
{
    public static class ActiveDaysHelper
    {
        public static bool IsActiveOn(ActiveDays activeDays, DayOfWeek dayOfWeek)
        {
            var flag = ToActiveDay(dayOfWeek);
            return (activeDays & flag) != 0;
        }

        public static ActiveDays ToActiveDay(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => ActiveDays.Monday,
                DayOfWeek.Tuesday => ActiveDays.Tuesday,
                DayOfWeek.Wednesday => ActiveDays.Wednesday,
                DayOfWeek.Thursday => ActiveDays.Thursday,
                DayOfWeek.Friday => ActiveDays.Friday,
                DayOfWeek.Saturday => ActiveDays.Saturday,
                DayOfWeek.Sunday => ActiveDays.Sunday,
                _ => ActiveDays.None
            };
        }

        public static DateTime? GetPreviousActiveDate(ActiveDays activeDays, DateTime today)
        {
            for (var i = 1; i <= 7; i++)
            {
                var candidate = today.AddDays(-i).Date;

                if (IsActiveOn(activeDays, candidate.DayOfWeek))
                {
                    return candidate;
                }
            }

            return null;
        }

    }
}
