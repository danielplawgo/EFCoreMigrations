using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using EFCoreMigrations.Web;
using EFCoreMigrations.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreMigrations.Migrator
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);

            result
                .WithParsed(Migrate);
        }

        private static void Migrate(Options options)
        {
            var serviceProvider = GetServiceCollection(options);

            var context = serviceProvider.GetRequiredService<DataContext>();

            Console.WriteLine("Migration is in progress...");
            Console.WriteLine("All:");
            Console.WriteLine(FormatMigrations(context.Database.GetMigrations()));
            Console.WriteLine("Applied:");
            Console.WriteLine(FormatMigrations(context.Database.GetAppliedMigrations()));
            Console.WriteLine("Pending:");
            Console.WriteLine(FormatMigrations(context.Database.GetPendingMigrations()));

            Console.WriteLine("Migrating...");
            context.Database.Migrate();

            Console.WriteLine("Database has been migrated");
        }

        private static string FormatMigrations(IEnumerable<string> migrations)
        {
            if (migrations.Any() == false)
            {
                return "\tNone";
            }

            return string.Join(Environment.NewLine, migrations.Select(m => $"\t{m}"));
        }

        private static ServiceProvider GetServiceCollection(Options options)
        {
            var serviceCollection = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    {"ConnectionStrings:DefaultConnection", options.ConnectionString}
                })
                .Build();
            serviceCollection.AddSingleton<IConfiguration>(config);

            var startup = new Startup(config);

            startup.ConfigureServices(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }
    }
}
