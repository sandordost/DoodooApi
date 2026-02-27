using DoodooApi.Models.CurrencyAccounts;
using DoodooApi.Models.Rewards;
using DoodooApi.Models.TodoItems;
using DoodooApi.Models.Users;
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
                claim.HasOne(c => c.User)
                    .WithMany(u => u.RewardClaims)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                claim.HasOne(c => c.Transaction);
            });
        }
    }
}
