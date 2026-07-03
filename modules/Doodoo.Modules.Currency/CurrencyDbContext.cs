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

                // Idempotency: at most one transaction per (source type, source id).
                transaction.HasIndex(t => new { t.SourceType, t.SourceIdGuid })
                    .IsUnique()
                    .HasFilter("\"SourceIdGuid\" IS NOT NULL");

                transaction.HasIndex(t => new { t.SourceType, t.SourceIdInt })
                    .IsUnique()
                    .HasFilter("\"SourceIdInt\" IS NOT NULL");
            });
        }
    }
}
