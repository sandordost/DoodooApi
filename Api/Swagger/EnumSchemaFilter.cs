using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DoodooApi.Swagger
{
    /// <summary>
    /// Appends each enum's name↔value mapping to its Swagger schema description so the
    /// numeric values are readable in the UI. Serialization stays integer-based, so the
    /// API contract does not change.
    /// </summary>
    public sealed class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;
            if (type is null || !type.IsEnum) return;
            if (schema is not OpenApiSchema concrete) return;

            var names = Enum.GetNames(type);
            var values = Enum.GetValues(type);

            var lines = new List<string>(names.Length);
            for (var i = 0; i < names.Length; i++)
            {
                var numeric = Convert.ToInt64(values.GetValue(i));
                lines.Add($"`{numeric}` = {names[i]}");
            }

            var mapping = string.Join("  \n", lines);
            concrete.Description = string.IsNullOrWhiteSpace(concrete.Description)
                ? mapping
                : $"{concrete.Description}\n\n{mapping}";
        }
    }
}
