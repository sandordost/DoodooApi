namespace Doodoo.Modules.Inventory.Contracts
{
    public enum InventoryOpCode
    {
        Success = 0,
        EntryNotFound = 1,
        NotACustomization = 2,
        NotAConsumable = 3,
        OutOfStock = 4,
        DefinitionNotFound = 5,
        CurrencyGrantFailed = 6,
        DuplicateKey = 7,
    }
}
