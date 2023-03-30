using AskQuestion.BLL.Repositories;
using AskQuestion.BLL.Repositories.Implementations;
using AskQuestion.BLL.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text;

namespace AskQuestion.WebApi.Extensions
{
    /// <summary>
    /// Методы расширения коллекции сервисов.
    /// </summary>
    internal static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Добавить сервис авторизации в контейнер.
        /// </summary>
        /// <param name="serviceCollection">Сервисы.</param>
        /// <param name="configuration">Конфиги</param>
        public static void ConfigureAuthentication(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddAuthentication().AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Jwt:Token").Value!)),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        public static void ConfigureSession(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".WebApi.Session";
            });
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Ask Question API",
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);

                options.OperationFilter<SecurityRequirementsOperationFilter>();
                options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.",
                    Type = SecuritySchemeType.ApiKey,
                });
            });
        }

        public static void ConfigureRepositories(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddScoped<IQuestionRepository, QuestionRepository>();
            serviceCollection.AddScoped<IFaqCategoryRepository, FaqCategoryRepository>();
            serviceCollection.AddScoped<IFaqEntryRepository, FaqEntryRepository>();
            serviceCollection.AddScoped<IFeedbackRepository, FeedbackRepository>();
            serviceCollection.AddScoped<IAreaRepository, AreaRepository>();
        }
    }
}
