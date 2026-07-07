using Doodoo.Messaging.Contracts;
using DoodooApi.Models.Main.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;

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
        IMessageBus bus) : UserManager<AppUser>(
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
            try
            {
                // Currency module owns the account. It creates it idempotently on this message.
                // InvokeAsync (rather than Publish) keeps registration synchronous so the account
                // exists by the time the response returns.
                await bus.InvokeAsync(new UserRegistered(user.Id));
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
