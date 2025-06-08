using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.AttributeBuilder;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;
using Piranha.Editorial.Services;
using Microsoft.AspNetCore.Identity;


using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using static Piranha.Editorial.Services.IEditorialWorkflowService;


var builder = WebApplication.CreateBuilder(args);

// Servi�os editoriais
builder.Services.AddScoped<Piranha.Editorial.Repositories.IWorkflowRepository, Piranha.Editorial.Repositories.WorkflowRepository>();
builder.Services.AddScoped<IEditorialWorkflowService, EditorialWorkflowService>();


// Adiciona Identity e servi�os necess�rios para UserManager funcionar

var meter = new Meter("RazorWeb");
builder.Services.AddSingleton(meter);


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

// Configura��o do Piranha CMS
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

// Inicializa��o do Piranha
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

// Seed de workflows e cria��o de roles personalizados (sem claims)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SQLiteDb>();
    RazorWeb.EditorialSeeder.Seed(db); // Seed do workflow

    // Seed s� dos roles (sem claims)
    await RazorWeb.SeedEditorialRoles.SeedAsync(scope.ServiceProvider);
}

app.MapControllers();

app.Run();
