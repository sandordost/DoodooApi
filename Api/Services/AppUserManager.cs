using DoodooApi.Models.Database;
using DoodooApi.Models.Main.CurrencyAccounts;
using DoodooApi.Models.Main.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoodooApi.Services
{
    public class AppUserManager(
        IUserStore<AppUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<AppUser> passwordHasher,
        IEnumerable<IUserValidator<AppUser>> userValidators,
        IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<AppUser>> logger,
        AppDbContext context) : UserManager<AppUser>(
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger)
    {
        public override async Task<IdentityResult> CreateAsync(AppUser user)
        {
            var result = await base.CreateAsync(user);

            if (!result.Succeeded)
                return result;

            return await EnsureCurrencyAccountAsync(user);
        }

        public override async Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            var result = await base.CreateAsync(user, password);

            if (!result.Succeeded)
                return result;

            return await EnsureCurrencyAccountAsync(user);
        }

        private async Task<IdentityResult> EnsureCurrencyAccountAsync(AppUser user)
        {
            var exists = await context.CurrencyAccounts
                .AnyAsync(ca => ca.OwnerId == user.Id);

            if (exists)
                return IdentityResult.Success;

            context.CurrencyAccounts.Add(new CurrencyAccount
            {
                OwnerId = user.Id,
                Gold = 0,
                Sapphires = 0
            });

            try
            {
                await context.SaveChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "CurrencyAccountCreateFailed",
                    Description = "User created, but creating the currency account failed."
                });
            }
        }
    }
}