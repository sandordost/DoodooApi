using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Main.Rewards
{
    public class DifficultyRewardRule
    {
        public int Id { get; set; }
        public ItemDifficulty Difficulty { get; set; }
        public decimal GoldAmount { get; set; }
        public int SapphireAmount { get; set; }
        public float SapphireChance { get; set; }
    }
}
