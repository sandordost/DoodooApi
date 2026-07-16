namespace Doodoo.Modules.Inventory.Contracts
{
    public sealed record InventoryOperationResult<T>(InventoryOpCode Code, T? Value)
    {
        public bool Success => Code == InventoryOpCode.Success;
        public static InventoryOperationResult<T> Ok(T value) => new(InventoryOpCode.Success, value);
        public static InventoryOperationResult<T> Fail(InventoryOpCode code) => new(code, default);
    }
}
