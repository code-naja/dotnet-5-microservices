﻿namespace Ordering.API.Extensions
{
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;

    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(
            this IHost host, 
            Action<TContext, IServiceProvider> seeder, 
            int? retry = 0) where TContext : DbContext
        {
            int retryForAvailability = retry.Value;

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                    InvokeSeeder(seeder, context, services);

                    logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
                }
                catch (SqlException ex)
                {
                    logger.LogError(ex, "An error occurred while migrating database used on context {DbContextName}", typeof(TContext).Name);

                    if (retryForAvailability < 3)
                    {
                        retryForAvailability++;
                        Thread.Sleep(5000);
                        MigrateDatabase<TContext>(host, seeder, retryForAvailability);
                    }
                }
            }

            return host;
        }

        private static void InvokeSeeder<TContext>(
            Action<TContext, IServiceProvider> seeder, 
            TContext context, 
            IServiceProvider services) where TContext : DbContext
        {
            context.Database.Migrate();
            seeder(context, services);
        }
    }
}
