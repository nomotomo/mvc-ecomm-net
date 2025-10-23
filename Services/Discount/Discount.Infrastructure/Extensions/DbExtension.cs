using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.Infrastructure.Extensions;

public static class DbExtension
{
    public static IHost MigrateDatabase<TContext>(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var config = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<TContext>>();
            try
            {
                logger.LogInformation("Migrating database");
                ApplyMigrations(config);
                logger.LogInformation("Database migrated successfully");
               
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the postgresql database");
                throw;
            }
        }
        
        return host;
    }

    private static void ApplyMigrations(IConfiguration config)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger("DatabaseMigration");
        logger.LogInformation("Applying migrations");
        var retry = 5;
        while (retry > 0)
        {
            try
            {
                using var connection = new NpgsqlConnection(config.GetValue<string>("DatabaseSettings:ConnectionString"));
                connection.Open();
                using var cmd = new NpgsqlCommand
                {
                    Connection = connection
                };
                cmd.CommandText = "DROP TABLE IF EXISTS Coupon";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY, 
                                                    ProductName VARCHAR(500) NOT NULL,
                                                    Description TEXT,
                                                    Amount INT)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Adidas Quick Force Indoor Badminton Shoes', 'Shoe Discount', 500);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Yonex VCORE Pro 100 A Tennis Racquet (270gm, Strung)', 'Racquet Discount', 700);";
                cmd.ExecuteNonQuery();
                // Exit loop if successful
                break; 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database");
                retry--;
                if (retry == 0)
                {
                    throw;
                }
                //Wait for 2 seconds
                Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}