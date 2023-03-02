using AskQuestion.DAL;
using AskQuestion.WebApi.Extensions;
using AskQuestion.WebApi.Helpers;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Сервисы контейнера.

builder.Services.AddControllers();

builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureSession();
builder.Services.ConfigureSwagger();

builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.ConfigureRepositories();

var app = builder.Build();

builder.Services.AddCors();

using var serviceScope = app.Services.CreateScope();

try
{
    RuntimeMigrations.Migrate(serviceScope.ServiceProvider);
}
catch (Exception exc)
{
    app.Logger.LogError(message: "Ошибка миграции базы данных: {exc}", exc);
    throw;
}

// Конвейер HTTP-запросов.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder.WithOrigins("http://localhost:8080")
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod());

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies[".WebApi"];

    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Add("Authorization", "Bearer " + token);
    }

    await next();
});

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapControllers();

app.Run();
