using DotNetEnv;
using whook.services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add Env &  Json configuration
Env.Load();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddSingleton<IConfiguration>(configuration);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<KeyService>();
builder.Services.AddSingleton<IDataContext,DataContext>();
builder.Services.AddSingleton<IScriptService,ScriptService>();
builder.Services.AddSingleton<IScriptLauncherService,ScriptLauncherService>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
