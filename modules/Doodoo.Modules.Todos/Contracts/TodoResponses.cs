using Doodoo.SharedKernel.Enums;

namespace Doodoo.Modules.Todos.Contracts
{
    public class DailyCheckResponse
    {
        public bool DailyHasReset { get; set; }
        public bool WeeklyHasReset { get; set; }
    }

}
