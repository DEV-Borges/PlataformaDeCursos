using Microsoft.AspNetCore.Identity;

namespace PlataformaDeCursos.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // 1. Criar Roles (Papéis)
        string[] roleNames = { "Admin", "Instructor", "Student" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Criar Usuário Admin Padrão
        var adminEmail = "admin@plataforma.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            // A senha real deve ser forte e, idealmente, vir do user-secrets na produção
            var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin@123!");
            if (createPowerUser.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
    }
}