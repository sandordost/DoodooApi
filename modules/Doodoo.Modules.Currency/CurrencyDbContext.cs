using DoodooApi.Models.Main.CurrencyAccounts;
using DoodooApi.Models.Main.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Doodoo.Modules.Currency
{
    public sealed class CurrencyDbContext(DbContextOptions<CurrencyDbContext> options) : DbContext(options)
    {
        public const string Schema = "currency";

        public DbSet<CurrencyAccount> CurrencyAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionRecord> TransactionRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<CurrencyAccount>(ca =>
            {
                ca.HasKey(c => c.Id);

                // One currency account per user (logical ref to Users module).
                ca.HasIndex(c => c.OwnerId).IsUnique();

                // Optimistic concurrency on balance updates via the Postgres system column xmin.
                ca.Property<uint>("xmin")
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();

                ca.HasMany(c => c.Transactions)
                    .WithOne(t => t.CurrencyAccount)
                    .HasForeignKey(t => t.CurrencyAccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>(transaction =>
            {
                transaction.HasKey(t => t.Id);

                transaction.HasMany(t => t.TransactionRecords)
                    .WithOne(r => r.Transaction)
                    .HasForeignKey(r => r.TransactionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Item completions are append-only: a recurring (daily/weekly) item is
                // completed many times and each completion is its own transaction, so this
                // is a plain lookup index, NOT unique. Retry/duplicate-delivery idempotency
                // is a messaging concern (Wolverine inbox), not a domain constraint.
                transaction.HasIndex(t => new { t.SourceType, t.SourceIdGuid });

                // Reward claims are one-shot: exactly one debit per claim id, so this stays unique.
                transaction.HasIndex(t => new { t.SourceType, t.SourceIdInt })
                    .IsUnique()
                    .HasFilter("\"SourceIdInt\" IS NOT NULL");
            });
        }
    }
}
