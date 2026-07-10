using Microsoft.AspNetCore.Identity;

namespace SakilaApp.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager  = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Create all required roles
        string[] roles = { "Administrador", "Empleado" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed admin user
        await SeedUserAsync(
            userManager,
            email: "admin@espe.edu.ec",
            password: "Admin123*",
            role: "Administrador");

        // Seed employee user (demonstrates different role permissions)
        await SeedUserAsync(
            userManager,
            email: "empleado@espe.edu.ec",
            password: "Empleado123*",
            role: "Empleado");
    }

    private static async Task SeedUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email    = email,
                EmailConfirmed = true   // skip email confirmation for seeded accounts
            };

            await userManager.CreateAsync(user, password);
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }
}