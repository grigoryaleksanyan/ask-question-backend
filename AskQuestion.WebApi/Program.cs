using AskQuestion.DAL;
using AskQuestion.WebApi.Extensions;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ������� ����������.

builder.Services.AddControllers();

builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureSession();
builder.Services.ConfigureSwagger();

builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.ConfigureRepositories();

builder.Services.AddScoped<EnsureVisitorIdAttribute>();

var app = builder.Build();

builder.Services.AddCors();

// �������� ��
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DataContext>();

    dbContext.Database.Migrate();
}

// �������� HTTP-��������.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder.WithOrigins("http://localhost:5000")
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod());

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.None
});

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapControllers();

app.Run();
