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

    public sealed record InventoryOperationResult<T>(InventoryOpCode Code, T? Value)
    {
        public bool Success => Code == InventoryOpCode.Success;
        public static InventoryOperationResult<T> Ok(T value) => new(InventoryOpCode.Success, value);
        public static InventoryOperationResult<T> Fail(InventoryOpCode code) => new(code, default);
    }
}
