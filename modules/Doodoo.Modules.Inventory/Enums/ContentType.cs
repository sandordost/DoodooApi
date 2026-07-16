namespace Doodoo.Modules.Inventory.Enums
{
    // How the frontend should interpret ItemDefinition.Content.
    public enum ContentType
    {
        None = 0,
        Spec = 1,        // declarative JSON skin-spec (layers + tokens + named animations)
        AssetToken = 2,  // reference to a built-in asset/preset (allowlist)
        Svg = 3,         // sanitized inline SVG (escape hatch)
        Html = 4,        // sanitized HTML (last-resort escape hatch)
    }
}
