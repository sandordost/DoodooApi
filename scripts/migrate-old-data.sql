-- ============================================================================
-- Cutover: move OLD single-schema (public) data into the new module schemas.
--
-- PREREQ: the old dump (doodoo-old.sql) has been loaded into the `public` schema
-- of THIS database, which already contains the (empty) module schemas created by
-- the migrator: users / currency / rewards / todos.
--
-- Safe: runs in one transaction. If anything fails (e.g. a duplicate that the new
-- unique indexes reject) the whole thing rolls back and nothing changes.
-- Idempotent-ish: re-running after a successful commit would insert duplicates, so
-- only run once against empty module tables.
-- ============================================================================

BEGIN;

-- Fail fast if the module tables are not empty (avoid double-import).
DO $$
BEGIN
    IF (SELECT count(*) FROM users."AspNetUsers") > 0
       OR (SELECT count(*) FROM currency."CurrencyAccounts") > 0
       OR (SELECT count(*) FROM todos."TodoItems") > 0
       OR (SELECT count(*) FROM rewards."Rewards") > 0 THEN
        RAISE EXCEPTION 'Module tables are not empty - aborting to avoid a double import.';
    END IF;
END $$;

-- ---------- USERS (ASP.NET Identity) ----------
INSERT INTO users."AspNetRoles" ("Id","Name","NormalizedName","ConcurrencyStamp")
SELECT "Id","Name","NormalizedName","ConcurrencyStamp" FROM public."AspNetRoles";

INSERT INTO users."AspNetUsers"
    ("Id","UserName","NormalizedUserName","Email","NormalizedEmail","EmailConfirmed","PasswordHash","SecurityStamp","ConcurrencyStamp","PhoneNumber","PhoneNumberConfirmed","TwoFactorEnabled","LockoutEnd","LockoutEnabled","AccessFailedCount","LastDailyReset","LastWeeklyReset")
SELECT "Id","UserName","NormalizedUserName","Email","NormalizedEmail","EmailConfirmed","PasswordHash","SecurityStamp","ConcurrencyStamp","PhoneNumber","PhoneNumberConfirmed","TwoFactorEnabled","LockoutEnd","LockoutEnabled","AccessFailedCount","LastDailyReset","LastWeeklyReset"
FROM public."AspNetUsers";

INSERT INTO users."AspNetRoleClaims" ("Id","RoleId","ClaimType","ClaimValue")
SELECT "Id","RoleId","ClaimType","ClaimValue" FROM public."AspNetRoleClaims";

INSERT INTO users."AspNetUserClaims" ("Id","UserId","ClaimType","ClaimValue")
SELECT "Id","UserId","ClaimType","ClaimValue" FROM public."AspNetUserClaims";

INSERT INTO users."AspNetUserLogins" ("LoginProvider","ProviderKey","ProviderDisplayName","UserId")
SELECT "LoginProvider","ProviderKey","ProviderDisplayName","UserId" FROM public."AspNetUserLogins";

INSERT INTO users."AspNetUserRoles" ("UserId","RoleId")
SELECT "UserId","RoleId" FROM public."AspNetUserRoles";

INSERT INTO users."AspNetUserTokens" ("UserId","LoginProvider","Name","Value")
SELECT "UserId","LoginProvider","Name","Value" FROM public."AspNetUserTokens";

-- ---------- CURRENCY ----------
INSERT INTO currency."CurrencyAccounts" ("Id","OwnerId","Gold","Sapphires")
SELECT "Id","OwnerId","Gold","Sapphires" FROM public."CurrencyAccounts";

INSERT INTO currency."Transactions" ("Id","SourceType","SourceIdInt","SourceIdGuid","CreatedTimestamp","CurrencyAccountId")
SELECT "Id","SourceType","SourceIdInt","SourceIdGuid","CreatedTimestamp","CurrencyAccountId" FROM public."Transactions";

INSERT INTO currency."TransactionRecords" ("Id","TransactionId","CurrencyType","Value")
SELECT "Id","TransactionId","CurrencyType","Value" FROM public."TransactionRecords";

-- ---------- REWARDS ----------
-- DifficultyRewardRules is already seeded by the migration, so it is NOT copied.
INSERT INTO rewards."Rewards" ("Id","Name","OwnerId","Description","Icon","IsDeleted")
SELECT "Id","Name","OwnerId","Description","Icon","IsDeleted" FROM public."Rewards";

INSERT INTO rewards."RewardCosts" ("Id","RewardId","CurrencyType","Amount")
SELECT "Id","RewardId","CurrencyType","Amount" FROM public."RewardCosts";

INSERT INTO rewards."RewardClaims" ("Id","RewardId","TransactionId","ClaimedAt","UserId")
SELECT "Id","RewardId","TransactionId","ClaimedAt","UserId" FROM public."RewardClaims";

-- ---------- TODOS ----------
INSERT INTO todos."TodoItems"
    ("Id","Title","Description","ItemDifficulty","OwnerId","CompletedTimestamp","DeletedTimestamp","ItemCategory","DailyStreak","LastCompletedTimestamp","LastWeeklyCheck","PreviousCompletedTimestamp","WeeklyStreak","Order","ActiveDays","LastResetDate")
SELECT "Id","Title","Description","ItemDifficulty","OwnerId","CompletedTimestamp","DeletedTimestamp","ItemCategory","DailyStreak","LastCompletedTimestamp","LastWeeklyCheck","PreviousCompletedTimestamp","WeeklyStreak","Order","ActiveDays","LastResetDate"
FROM public."TodoItems";

-- ---------- Reset identity sequences (int PKs) so future inserts don't collide ----------
SELECT setval(pg_get_serial_sequence('users."AspNetRoleClaims"','Id'),   COALESCE((SELECT MAX("Id") FROM users."AspNetRoleClaims"),1),   (SELECT MAX("Id") FROM users."AspNetRoleClaims")   IS NOT NULL);
SELECT setval(pg_get_serial_sequence('users."AspNetUserClaims"','Id'),   COALESCE((SELECT MAX("Id") FROM users."AspNetUserClaims"),1),   (SELECT MAX("Id") FROM users."AspNetUserClaims")   IS NOT NULL);
SELECT setval(pg_get_serial_sequence('rewards."Rewards"','Id'),          COALESCE((SELECT MAX("Id") FROM rewards."Rewards"),1),          (SELECT MAX("Id") FROM rewards."Rewards")          IS NOT NULL);
SELECT setval(pg_get_serial_sequence('rewards."RewardCosts"','Id'),      COALESCE((SELECT MAX("Id") FROM rewards."RewardCosts"),1),      (SELECT MAX("Id") FROM rewards."RewardCosts")      IS NOT NULL);
SELECT setval(pg_get_serial_sequence('rewards."RewardClaims"','Id'),     COALESCE((SELECT MAX("Id") FROM rewards."RewardClaims"),1),     (SELECT MAX("Id") FROM rewards."RewardClaims")     IS NOT NULL);
SELECT setval(pg_get_serial_sequence('currency."TransactionRecords"','Id'), COALESCE((SELECT MAX("Id") FROM currency."TransactionRecords"),1), (SELECT MAX("Id") FROM currency."TransactionRecords") IS NOT NULL);

-- ---------- Verification (row counts new vs old) ----------
SELECT 'users.AspNetUsers'          AS tbl, (SELECT count(*) FROM users."AspNetUsers")          AS new_rows, (SELECT count(*) FROM public."AspNetUsers")          AS old_rows
UNION ALL SELECT 'users.AspNetRoles',         (SELECT count(*) FROM users."AspNetRoles"),         (SELECT count(*) FROM public."AspNetRoles")
UNION ALL SELECT 'currency.CurrencyAccounts', (SELECT count(*) FROM currency."CurrencyAccounts"), (SELECT count(*) FROM public."CurrencyAccounts")
UNION ALL SELECT 'currency.Transactions',     (SELECT count(*) FROM currency."Transactions"),     (SELECT count(*) FROM public."Transactions")
UNION ALL SELECT 'currency.TransactionRecords',(SELECT count(*) FROM currency."TransactionRecords"),(SELECT count(*) FROM public."TransactionRecords")
UNION ALL SELECT 'rewards.Rewards',           (SELECT count(*) FROM rewards."Rewards"),           (SELECT count(*) FROM public."Rewards")
UNION ALL SELECT 'rewards.RewardCosts',       (SELECT count(*) FROM rewards."RewardCosts"),       (SELECT count(*) FROM public."RewardCosts")
UNION ALL SELECT 'rewards.RewardClaims',      (SELECT count(*) FROM rewards."RewardClaims"),      (SELECT count(*) FROM public."RewardClaims")
UNION ALL SELECT 'todos.TodoItems',           (SELECT count(*) FROM todos."TodoItems"),           (SELECT count(*) FROM public."TodoItems")
ORDER BY tbl;

COMMIT;

-- ============================================================================
-- After you have verified the counts above match, drop the old public tables.
-- Run this SEPARATELY (not part of the transaction) once you're satisfied:
--
--   DROP TABLE IF EXISTS
--     public."AspNetRoleClaims", public."AspNetUserClaims", public."AspNetUserLogins",
--     public."AspNetUserRoles", public."AspNetUserTokens", public."AspNetUsers",
--     public."AspNetRoles", public."TransactionRecords", public."Transactions",
--     public."CurrencyAccounts", public."RewardClaims", public."RewardCosts",
--     public."Rewards", public."DifficultyRewardRules", public."TodoItems",
--     public."__EFMigrationsHistory" CASCADE;
-- ============================================================================
