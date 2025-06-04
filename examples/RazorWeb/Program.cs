using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.AttributeBuilder;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;
using Piranha.Manager;
using Piranha.Editorial.Services;

var builder = WebApplication.CreateBuilder(args);

// Serviços editoriais
builder.Services.AddScoped<Piranha.Editorial.Repositories.IWorkflowRepository, Piranha.Editorial.Repositories.WorkflowRepository>();
builder.Services.AddScoped<IEditorialWorkflowService, EditorialWorkflowService>();

// Configuração do Piranha CMS
builder.AddPiranha(options =>
{
    options.AddRazorRuntimeCompilation = true;

    options.UseCms();
    options.UseManager();

    options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
    options.UseImageSharp();
    options.UseTinyMCE();
    options.UseMemoryCache();

    var connectionString = builder.Configuration.GetConnectionString("piranha");

    options.UseEF<SQLiteDb>(db => db.UseSqlite(connectionString));
    options.UseIdentityWithSeed<IdentitySQLiteDb>(db => db.UseSqlite(connectionString));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Inicialização do Piranha
app.UsePiranha(options =>
{
    App.Init(options.Api);

    new ContentTypeBuilder(options.Api)
        .AddAssembly(typeof(Program).Assembly)
        .Build()
        .DeleteOrphans();

    EditorConfig.FromFile("editorconfig.json");

    options.UseManager();
    options.UseTinyMCE();
    options.UseIdentity();
});

// Seed de workflows e criação de roles personalizados (sem claims)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SQLiteDb>();
    RazorWeb.EditorialSeeder.Seed(db); // Seed do workflow

    // Seed só dos roles (sem claims)
    await RazorWeb.SeedEditorialRoles.SeedAsync(scope.ServiceProvider);
}

app.MapControllers();

app.Run();
