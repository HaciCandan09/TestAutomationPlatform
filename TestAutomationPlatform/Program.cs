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

//Defectmanagement
builder.Services.AddScoped<IDefectService, DefectService>();

// Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

var app = builder.Build();

// Hangfire dashboard
app.UseHangfireDashboard();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();



//screenshots ophalen
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Screenshots")),
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




BackgroundJob.Enqueue<RunService>(x => x.ExecuteRun("Dev"));

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

app.Run();