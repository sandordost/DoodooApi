using Doodoo.Messaging.Contracts;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Main.CurrencyAccounts;
using DoodooApi.Models.Requests.Transactions;
using DoodooApi.Services;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Doodoo.Modules.Currency
{
    // Currency is the single writer of the ledger. All money movements enter via these
    // in-process Wolverine handlers, invoked by the Todos / Rewards / Users modules.
    public static class CurrencyHandler
    {
        // Users -> Currency (event). Idempotently create the account for a new user.
        public static async Task Handle(UserRegistered message, CurrencyDbContext db)
        {
            var exists = await db.CurrencyAccounts.AnyAsync(ca => ca.OwnerId == message.UserId);
            if (exists) return;

            db.CurrencyAccounts.Add(new CurrencyAccount
            {
                OwnerId = message.UserId,
                Gold = 0,
                Sapphires = 0
            });

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Unique index on OwnerId lost a race; the account already exists.
            }
        }

        // Todos -> Currency (request/response). Grant the difficulty-based reward for a completion.
        public static async Task<ItemCompletionRewardResult> Handle(
            GrantItemCompletionReward message,
            CurrencyDbContext db,
            TransactionService transactionService,
            IMessageBus bus)
        {
            var account = await db.CurrencyAccounts
                .FirstOrDefaultAsync(ca => ca.OwnerId == message.UserId);

            if (account == null)
                return new ItemCompletionRewardResult(TransactionResponseCode.CurrencyAccountNotFound, null, 0, 0);

            // Append-only: recurring items are completed repeatedly, so each completion is a
            // new transaction. The Todos module guards against double-completion within a period
            // (CompletedTimestamp), so we always create here.
            // Rewards module owns the difficulty rule + deterministic RNG.
            var amounts = await bus.InvokeAsync<DifficultyRewardAmounts>(
                new CalculateDifficultyReward(message.UserId, message.ItemId, message.Difficulty, message.CompletedAtUtc));

            var records = new List<TransactionRecordRequest>();
            if (amounts.Gold != 0)
                records.Add(new TransactionRecordRequest { CurrencyType = CurrencyType.Gold, Value = amounts.Gold });
            if (amounts.Sapphires != 0)
                records.Add(new TransactionRecordRequest { CurrencyType = CurrencyType.Sapphire, Value = amounts.Sapphires });

            var response = await transactionService.MakeTransactionAsync(new CreateTransactionRequest
            {
                SourceType = TransactionSourceType.ItemCompletion,
                SourceIdGuid = message.ItemId,
                CurrencyAccountId = account.Id,
                TransactionRecords = records
            });

            return new ItemCompletionRewardResult(
                response.ResponseCode, response.TransactionId, amounts.Gold, amounts.Sapphires);
        }

        // Todos -> Currency (compensation). Undo a completion grant.
        public static async Task<RevertResult> Handle(
            RevertItemCompletionReward message,
            TransactionService transactionService)
        {
            var transaction = await transactionService.GetTransactionBySourceAsync(
                TransactionSourceType.ItemCompletion, message.ItemId);

            if (transaction == null)
                return new RevertResult(TransactionResponseCode.NoTransactionFound);

            var code = await transactionService.UndoTransactionAsync(transaction.Id);
            return new RevertResult(code);
        }

        // Rewards -> Currency (request/response). Debit currency for a reward claim.
        public static async Task<RewardClaimDebitResult> Handle(
            DebitForRewardClaim message,
            CurrencyDbContext db,
            TransactionService transactionService)
        {
            var account = await db.CurrencyAccounts
                .FirstOrDefaultAsync(ca => ca.OwnerId == message.UserId);

            if (account == null)
                return new RewardClaimDebitResult(TransactionResponseCode.CurrencyAccountNotFound, null);

            // Idempotency: this claim was already debited.
            var existing = await transactionService.GetTransactionBySourceAsync(
                TransactionSourceType.RewardClaim, message.ClaimId);

            if (existing != null)
                return new RewardClaimDebitResult(TransactionResponseCode.Created, existing.Id);

            var requiredGold = message.Costs
                .Where(c => c.CurrencyType == CurrencyType.Gold)
                .Sum(c => c.Value);

            var requiredSapphires = message.Costs
                .Where(c => c.CurrencyType == CurrencyType.Sapphire)
                .Sum(c => (int)c.Value);

            if (account.Gold < requiredGold || account.Sapphires < requiredSapphires)
                return new RewardClaimDebitResult(TransactionResponseCode.InsufficientFunds, null);

            var response = await transactionService.MakeTransactionAsync(new CreateTransactionRequest
            {
                SourceType = TransactionSourceType.RewardClaim,
                SourceIdInt = message.ClaimId,
                CurrencyAccountId = account.Id,
                TransactionRecords = [.. message.Costs.Select(c => new TransactionRecordRequest
                {
                    CurrencyType = c.CurrencyType,
                    Value = -c.Value
                })]
            });

            return new RewardClaimDebitResult(response.ResponseCode, response.TransactionId);
        }

        // Rewards -> Currency (request/response). Refund a previously claimed reward.
        public static async Task<RefundResult> Handle(
            RefundRewardClaim message,
            TransactionService transactionService)
        {
            var code = await transactionService.UndoTransactionAsync(message.TransactionId);
            return new RefundResult(code);
        }
    }
}
