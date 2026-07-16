using Microsoft.OpenApi;

namespace DoodooApi.Swagger
{
    public static class AdminOnlySwaggerFilter
    {
        public static void HideAdminPathsForNonAdmins(OpenApiDocument document, HttpRequest request)
        {
            if (request.HttpContext.User.Identity?.IsAuthenticated == true
                && request.HttpContext.User.IsInRole("Admin"))
            {
                return;
            }

            var adminPaths = document.Paths
                .Where(path => IsAdminPath(path.Key))
                .Select(path => path.Key)
                .ToList();

            foreach (var path in adminPaths)
            {
                document.Paths.Remove(path);
            }

            if (document.Tags is not null)
            {
                var adminTags = document.Tags
                    .Where(tag => IsAdminTag(tag.Name))
                    .ToList();

                foreach (var tag in adminTags)
                {
                    document.Tags.Remove(tag);
                }
            }
        }

        private static bool IsAdminPath(string path) =>
            path.StartsWith("/api/admin/", StringComparison.OrdinalIgnoreCase);

        private static bool IsAdminTag(string? tag) =>
            string.Equals(tag, "AdminInventory", StringComparison.OrdinalIgnoreCase)
            || (tag?.StartsWith("Admin", StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
