using DoodooApi.Models.Enums;
using DoodooApi.Models.Main.CurrencyAccounts;
using DoodooApi.Models.Main.Rewards;
using DoodooApi.Models.Main.TodoItems;
using DoodooApi.Models.Main.Transactions;
using DoodooApi.Models.Main.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Models.Database
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<DifficultyRewardRule> DifficultyRewardRules { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<RewardClaim> RewardClaims { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CurrencyAccount> CurrencyAccounts { get; set; }
        public DbSet<RewardCost> RewardCosts { get; set; }
        public DbSet<TransactionRecord> TransactionRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(user =>
            {
                user.HasIndex(u => u.UserName).IsUnique();
                user.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<TodoItem>(todo =>
            {
                todo.HasOne(t => t.Owner)
                    .WithMany(u => u.TodoItems)
                    .HasForeignKey(t => t.OwnerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CurrencyAccount>(ca =>
            {
                ca.HasOne(c => c.Owner)
                .WithOne(u => u.CurrencyAccount)
                .HasForeignKey<CurrencyAccount>(cca => cca.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>(transaction =>
            {
                transaction.HasOne(t => t.CurrencyAccount)
                    .WithMany(ca => ca.Transactions)
                    .HasForeignKey(tr => tr.CurrencyAccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                transaction.HasMany(t => t.TransactionRecords)
                    .WithOne(r => r.Transaction)
                    .HasForeignKey(r => r.TransactionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Reward>(reward =>
            {
                reward.HasOne(r => r.Owner)
                    .WithMany(u => u.Rewards)
                    .HasForeignKey(r => r.OwnerId)
                    .OnDelete(DeleteBehavior.Cascade);

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

                claim.HasOne(c => c.User)
                    .WithMany(u => u.RewardClaims)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                claim.HasOne(c => c.Transaction)
                    .WithMany()
                    .HasForeignKey(c => c.TransactionId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
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
