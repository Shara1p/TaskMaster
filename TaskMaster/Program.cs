using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Middleware.Extensions;
using TaskMaster.Services;

// When running from the repo root (dotnet run --project TaskMaster.csproj),
// the current directory is the root, but appsettings.json lives in TaskMaster/.
// Set ContentRootPath so configuration files are resolved correctly.
var contentRoot = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(contentRoot, "appsettings.json")))
{
    contentRoot = Path.Combine(contentRoot, "TaskMaster");
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
// Get database path - try root first (where app runs from based on cwd in launch.json)
// then fallback to TaskMaster subdirectory
var dbPath = "taskmaster.db";
if (!File.Exists(dbPath))
{
    dbPath = Path.Combine("TaskMaster", "taskmaster.db");
}
var fullDbPath = Path.GetFullPath(dbPath);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite($"Data Source={fullDbPath}"));

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRequestLogging();

app.MapControllers();

app.Run();
