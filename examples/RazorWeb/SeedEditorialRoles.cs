using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Piranha.AspNetCore.Identity.Data; // Role do Piranha CMS

namespace RazorWeb
{
    public static class SeedEditorialRoles
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            var claimsPorRole = new Dictionary<string, string[]>
            {
                ["Autor"] = new[]
                {
                    "PiranhaPagesAdd", "PiranhaPagesEdit", "PiranhaPagesSave", "PiranhaPages",
                    "PiranhaPagePreview", "PiranhaAdmin", "PiranhaPostPreview", "PiranhaContentAdd",
                    "PiranhaContentDelete", "PiranhaContentEdit", "PiranhaContent", "PiranhaContentSave",
                    "LanguageAdd", "LanguageDelete", "LanguageEdit", "Language",
                    "PiranhaMediaAdd", "PiranhaMediaAddFolder", "PiranhaMediaDeleteFolder",
                    "PiranhaMediaEdit", "PiranhaMedia", "PiranhaMediaDelete",
                    "PiranhaPostsAdd", "PiranhaPostsEdit", "PiranhaPosts", "PiranhaPostsSave","PiranhaPagesDelete"
                },

                ["Editor"] = new[]
                {
                    "PiranhaPagesAdd", "PiranhaPagesEdit", "PiranhaPagesSave", "PiranhaPages",
                    "PiranhaPagePreview", "PiranhaSitesAdd", "PiranhaSitesEdit", "PiranhaSitesSave",
                    "PiranhaSitesDelete", "PiranhaAdmin", "PiranhaAliasesDelete", "PiranhaAliasesEdit",
                    "PiranhaAliases", "PiranhaCommentsApprove", "PiranhaCommentsDelete", "PiranhaComments",
                    "PiranhaContentAdd", "PiranhaContentDelete", "PiranhaContentEdit", "PiranhaContent",
                    "PiranhaContentSave", "LanguageAdd", "LanguageDelete", "LanguageEdit", "Language",
                    "PiranhaMediaAdd", "PiranhaMediaAddFolder", "PiranhaMediaDelete", "PiranhaMediaDeleteFolder",
                    "PiranhaMediaEdit", "PiranhaMedia", "PiranhaPostsAdd", "PiranhaPostsDelete", "PiranhaPostsEdit",
                    "PiranhaPosts", "PiranhaPostsSave", "PiranhaPostPreview", "PiranhaPagesDelete","PiranhaSites","PiranhaPostsPublish"
                },

                ["Jurista"] = new[]
                {
                    "PiranhaPages", "PiranhaPagePreview", "PiranhaAdmin", "PiranhaPostPreview", "PiranhaPosts",
                    "PiranhaSites","PiranhaPagesEdit"
                },

                ["Diretor"] = new[]
                {
                    "PiranhaPages", "PiranhaPagesEdit", "PiranhaPagesSave", "PiranhaPagesPublish", "PiranhaPagePreview",
                    "PiranhaAdmin", "PiranhaAliasesDelete", "PiranhaAliasesEdit", "PiranhaAliases",
                    "PiranhaCommentsApprove", "PiranhaCommentsDelete", "PiranhaComments",
                    "PiranhaContentAdd", "PiranhaContentDelete", "PiranhaContentEdit", "PiranhaContent",
                    "PiranhaContentSave", "PiranhaMediaAdd", "PiranhaMediaAddFolder", "PiranhaMediaDelete",
                    "PiranhaMediaDeleteFolder", "PiranhaMediaEdit", "PiranhaMedia", "PiranhaPagesAdd",
                    "PiranhaPagesDelete", "PiranhaPostsAdd", "PiranhaPostsDelete", "PiranhaPostsEdit",
                    "PiranhaPosts", "PiranhaPostsPublish", "PiranhaPostsSave", "PiranhaSitesAdd",
                    "PiranhaSitesDelete", "PiranhaSitesEdit", "PiranhaSites", "PiranhaSitesSave",
                    "PiranhaUsersAdd", "PiranhaUsersDelete", "PiranhaUsersEdit", "PiranhaUsers",
                    "PiranhaUsersSave", "PiranhaPostPreview", "LanguageAdd", "LanguageDelete",
                    "LanguageEdit", "Language"
                }
            };

            foreach (var (roleName, claims) in claimsPorRole)
            {
                // Cria o role se não existir
                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    role = new Role { Name = roleName };
                    await roleManager.CreateAsync(role);
                }

                // Verifica e adiciona claims em falta
                var existingClaims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in claims)
                {
                    if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == claim))
                    {
                        await roleManager.AddClaimAsync(role, new Claim(claim, claim));
                    }
                }
            }
        }
    }

}
