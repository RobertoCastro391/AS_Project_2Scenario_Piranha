using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.AttributeBuilder;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;
using Piranha.Editorial.Services;

using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<Piranha.Editorial.Repositories.IWorkflowRepository, Piranha.Editorial.Repositories.WorkflowRepository>();
builder.Services.AddScoped<IEditorialWorkflowService, EditorialWorkflowService>();

var meter = new Meter("RazorWeb");
builder.Services.AddSingleton(meter);

var counter = meter.CreateCounter<long>("basket_created_count", description: "Number of baskets created or updated.");
counter.Add(1);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("RazorWeb"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("RazorWeb.Service")
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri("http://localhost:4317");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("RazorWeb")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4317");
            });
    });

//builder.Logging.AddOpenTelemetry(options =>
//{
//    options.IncludeFormattedMessage = true;
//    options.ParseStateValues = true;
//    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("RazorWeb"));

//    // Exporta logs via OTLP para o Collector
//    options.AddOtlpExporter(exporterOptions =>
//    {
//        exporterOptions.Endpoint = new Uri("http://localhost:4317");
//    });
//});

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

    options.UseIdentityWithSeed<IdentitySQLiteDb>(db => db.UseSqlite(connectionString));

    /**
     * Here you can configure the different permissions
     * that you want to use for securing content in the
     * application.
    options.UseSecurity(o =>
    {
        o.UsePermission("WebUser", "Web User");
    });
     */

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