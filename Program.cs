
using ChatApp.API.Hubs;
using ChatApp.API.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Stream.Schema;

var builder = WebApplication.CreateBuilder(args);

Env.Load();


builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<AuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FlutterPolicy", policy =>
        policy
            .WithOrigins("http://localhost:*", "https://localhost:*")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

// Custom AuthService if you created one
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors("FlutterPolicy");

//app.UseHttpsRedirection();


app.MapGet("/", () => new { 
    message = "Hello World!", 
    status = "Success",
    timestamp = DateTime.UtcNow 
});

app.MapHub<ChatHub>("/hubs/chat");


app.UseAuthorization();
app.MapControllers();


app.Run();

