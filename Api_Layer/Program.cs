using Api_Layer.DependencyInjection;
using Api_Layer.Extensions;
using Api_Layer.Middlewares;
using Business_Layer.Mappings;
using Data_Access_Layer.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddCacheService(builder.Configuration);
builder.Services.AddAutoMapper();
builder.Services.AddServices();
builder.Services.AddIdentity(builder.Configuration);
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
app.Run();
