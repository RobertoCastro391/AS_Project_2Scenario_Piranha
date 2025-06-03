using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.AttributeBuilder;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;
using Piranha.Manager;
using Piranha.Editorial.Services;
using Piranha.Editorial.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<Piranha.Editorial.Repositories.IWorkflowRepository, Piranha.Editorial.Repositories.WorkflowRepository>();
builder.Services.AddScoped<IEditorialWorkflowService, EditorialWorkflowService>();

// Add editorial permissions
builder.Services.AddEditorialPermissions();



builder.AddPiranha(options =>
{
    /**
     * This will enable automatic reload of .cshtml
     * without restarting the application. However since
     * this adds a slight overhead it should not be
     * enabled in production.
     */
    options.AddRazorRuntimeCompilation = true;

    options.UseCms();
    options.UseManager();

    options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
    options.UseImageSharp();
    options.UseTinyMCE();
    options.UseMemoryCache();    var connectionString = builder.Configuration.GetConnectionString("piranha");
    options.UseEF<SQLiteDb>(db => db.UseSqlite(connectionString));

    options.UseIdentityWithSeed<IdentitySQLiteDb>(db => db.UseSqlite(connectionString));    /**
     * Here you can configure the different permissions
     * that you want to use for securing content in the
     * application.
     */
    options.UseSecurity(o =>
    {
        // Add editorial permissions
        o.UsePermission(Piranha.Editorial.Permissions.Workflow, "Editorial Workflow");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowView, "View Editorial Workflow");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowAuthor, "Autor Role");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowEditor, "Editor Role");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowLegal, "Jurista Role");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowDirector, "Diretor Role");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowSubmitForReview, "Submit for Review");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowApproveEditorial, "Approve Editorial");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowRejectEditorial, "Reject Editorial");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowSubmitLegal, "Submit for Legal Review");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowApproveLegal, "Approve Legal");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowRejectLegal, "Reject Legal");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowPublish, "Publish Content");
        o.UsePermission(Piranha.Editorial.Permissions.WorkflowUnpublish, "Unpublish Content");
    });

    /**
     * Here you can specify the login url for the front end
     * application. This does not affect the login url of
     * the manager interface.
    options.LoginUrl = "login";
     */
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UsePiranha(options =>
{
    // Initialize Piranha
    App.Init(options.Api);

    using (var scope = app.Services.CreateScope())
    {        var db = scope.ServiceProvider.GetRequiredService<SQLiteDb>();
        RazorWeb.EditorialSeeder.Seed(db);
    }

    // Build content types
    new ContentTypeBuilder(options.Api)
        .AddAssembly(typeof(Program).Assembly)
        .Build()
        .DeleteOrphans();

    // Configure Tiny MCE
    EditorConfig.FromFile("editorconfig.json");

    options.UseManager();
    options.UseTinyMCE();
    options.UseIdentity();
});

app.MapControllers();

app.Run();