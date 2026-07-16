using System.ComponentModel.DataAnnotations;

namespace Doodoo.Modules.Inventory.Contracts
{
    /// <summary>
    /// Grant an item to one user (by <see cref="UserId"/> or <see cref="Email"/>), or to everyone
    /// when <see cref="All"/> is true.
    /// </summary>
    public sealed class GrantItemRequest
    {
        public Guid? UserId { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public bool All { get; set; }
        [Required] public string DefinitionKey { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
    }
}
