using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    public static class DependencyInjection
    {
        public static void AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<Db>(optionsAction =>
            {
                optionsAction.UseSqlite($"Data Source=db.db");
            });

        }

        public static void UseDataAccess(this IServiceProvider serviceProvider)
        {
            var factory = serviceProvider.GetService<IDbContextFactory<Db>>();
            var db = factory.CreateDbContext();
            db.Database.Migrate();
        }
    }
}