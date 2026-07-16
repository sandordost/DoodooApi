namespace Doodoo.Messaging.Contracts
{
    // Users -> Currency (event, via outbox).
    public record UserRegistered(Guid UserId);
}
