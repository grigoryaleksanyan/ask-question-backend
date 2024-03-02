using AskQuestion.DAL;
using Microsoft.EntityFrameworkCore;

namespace AskQuestion.WebApi.Helpers
{
    /// <summary>
    /// Выполнение автоматической миграции.
    /// </summary>
    public static class RuntimeMigrations
    {
        /// <summary>
        /// Применить миграцию.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public static void Migrate(IServiceProvider serviceProvider)
        {
            serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            var appContextService = serviceProvider.GetRequiredService<DataContext>();

            appContextService.Database.Migrate();

            //using var connection = (NpgsqlConnection)appContextService.Database.GetDbConnection();
            //connection.Open();
            //connection.ReloadTypes();
        }
    }
}
