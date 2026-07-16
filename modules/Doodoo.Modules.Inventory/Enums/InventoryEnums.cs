namespace Doodoo.Modules.Inventory.Enums
{
    public enum ItemKind
    {
        Customization = 0,
        Consumable = 1,
        Membership = 2,
    }

    // Frontend-owned anchor points a customization decorates (never replaces).
    public enum ItemSlot
    {
        None = 0,
        AppBackground = 1,
        TodoCardBackground = 2,
        NavButtons = 3,
        ProfileButton = 4,
        CompletionAnimation = 5,
    }

    // How the frontend should interpret ItemDefinition.Content.
    public enum ContentType
    {
        None = 0,
        Spec = 1,        // declarative JSON skin-spec (layers + tokens + named animations)
        AssetToken = 2,  // reference to a built-in asset/preset (allowlist)
        Svg = 3,         // sanitized inline SVG (escape hatch)
        Html = 4,        // sanitized HTML (last-resort escape hatch)
    }

    public enum ConsumableEffect
    {
        None = 0,
        GrantGold = 1,
        GrantSapphires = 2,
    }
}
