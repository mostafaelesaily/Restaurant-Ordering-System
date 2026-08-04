using Api_Layer.Extensions;
using Api_Layer.Middlewares;
using Application;
using Infrastructure.DependencyInjection;
using Resturant_Ordering_System.Infrastructre.BackgroundServices;
using Resturant_Ordering_System.Infrastructre.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddHostedService<ServerTimeNotification>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAppDI();
builder.Services.AddCaching(builder.Configuration);
builder.Services.AddFileServices();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
await app.SeedDatabaseAsync();
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.Run();
