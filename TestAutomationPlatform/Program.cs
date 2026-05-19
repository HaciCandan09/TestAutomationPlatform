using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Repository;
using TestAutomationPlatform.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services
builder.Services.AddScoped<TestExecutionService>();
builder.Services.AddScoped<ScriptParser>();
builder.Services.AddScoped<RunService>();

// Repository
builder.Services.AddScoped<IScriptRepository, ScriptRepository>();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

var app = builder.Build();

var screenshotsPath = Path.Combine(app.Environment.ContentRootPath, "Screenshots");
Directory.CreateDirectory(screenshotsPath);

// Hangfire dashboard
app.UseHangfireDashboard();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(screenshotsPath),
    RequestPath = "/screenshots"
});

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

if (app.Configuration.GetValue<bool>("Hangfire:EnqueueStartupRun"))
{
    BackgroundJob.Enqueue<RunService>(x => x.ExecuteRun("Dev"));
}

if (app.Configuration.GetValue("Hangfire:EnableRecurringJobs", true))
{
    RecurringJob.AddOrUpdate<RunService>(
        "run-tests-dev",
        x => x.ExecuteRun("Dev"),
        Cron.MinuteInterval(5)
    );

    RecurringJob.AddOrUpdate<RunService>(
        "run-tests-preprod",
        x => x.ExecuteRun("Preprod"),
        Cron.HourInterval(1)
    );

    RecurringJob.AddOrUpdate<RunService>(
        "run-tests-prod",
        x => x.ExecuteRun("Prod"),
        Cron.Daily()
    );
}

app.Run();
