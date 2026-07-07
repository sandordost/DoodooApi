using DoodooApi.Models.Enums;
using DoodooApi.Models.Main.Rewards;
using Microsoft.EntityFrameworkCore;

namespace Doodoo.Modules.Rewards
{
    public sealed class RewardsDbContext(DbContextOptions<RewardsDbContext> options) : DbContext(options)
    {
        public const string Schema = "rewards";

        public DbSet<Reward> Rewards { get; set; }
        public DbSet<RewardCost> RewardCosts { get; set; }
        public DbSet<RewardClaim> RewardClaims { get; set; }
        public DbSet<DifficultyRewardRule> DifficultyRewardRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<Reward>(reward =>
            {
                reward.HasKey(r => r.Id);
                reward.HasIndex(r => r.OwnerId);

                reward.HasMany(r => r.Claims)
                    .WithOne(c => c.Reward)
                    .HasForeignKey(c => c.RewardId)
                    .OnDelete(DeleteBehavior.Cascade);

                reward.HasMany(r => r.RewardCosts)
                    .WithOne(rc => rc.Reward)
                    .HasForeignKey(rc => rc.RewardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RewardClaim>(claim =>
            {
                claim.HasKey(c => c.Id);
                claim.Property(c => c.Id).ValueGeneratedOnAdd();
                claim.HasIndex(c => c.UserId);
            });

            modelBuilder.Entity<DifficultyRewardRule>().HasData(SeedDifficultyRewardRules);
        }

        private static List<DifficultyRewardRule> SeedDifficultyRewardRules => [
            new(){
                Id = 1,
                Difficulty = ItemDifficulty.Trivial,
                GoldAmount = 0.4m,
                SapphireAmount = 1,
                SapphireChance = 0.01f
            },
            new(){
                Id = 2,
                Difficulty = ItemDifficulty.Easy,
                GoldAmount = 0.7m,
                SapphireAmount = 1,
                SapphireChance = 0.05f
            },
            new(){
                Id = 3,
                Difficulty = ItemDifficulty.Medium,
                GoldAmount = 1,
                SapphireAmount = 1,
                SapphireChance = 0.1f
            },
            new(){
                Id = 4,
                Difficulty = ItemDifficulty.Hard,
                GoldAmount = 1.3m,
                SapphireAmount = 1,
                SapphireChance = 0.15f
            },
        ];
    }
}
