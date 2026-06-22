using HealthSync.Core.Entities.Identity;
using HealthSync.Infrastructure.Data;
using HealthSync.Infrastructure.Seeds;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HealthSync.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var hasher = services.GetRequiredService<PasswordHasher<ApplicationUser>>();
        await DatabaseSeeder.SeedAsync(context, hasher);
    }
}
