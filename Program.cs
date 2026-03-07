
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Stream.Schema;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<PasswordHasher<Users>>();

// Custom AuthService if you created one
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


app.MapGet("/", () => new { 
    message = "Hello World!", 
    status = "Success",
    timestamp = DateTime.UtcNow 
});

app.Run();

