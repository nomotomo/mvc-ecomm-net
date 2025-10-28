using Dapper;
using Discount.Core.Entities;
using Discount.Core.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Discount.Infrastructure.Repositories;

public class DiscountRepository(IConfiguration configuration) : IDiscountRepository
{
    private readonly string _connectionString = configuration.GetValue<string>("DatabaseSettings:ConnectionString")
                                                ?? throw new InvalidOperationException("Connection string not configured");
    private NpgsqlConnection GetPGConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }   
    public async Task<Coupon> GetDiscount(string productName)
    {
        await using var connection = GetPGConnection();
        var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>("SELECT * FROM Coupon WHERE ProductName = @ProductName", new { ProductName = productName });
        return coupon ?? new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Available" }; 
    }

    public async Task<bool> CreateDiscount(Coupon coupon)
    {
        await using var connection = GetPGConnection();
        var affected = await connection.ExecuteAsync("INSERT INTO Coupon (ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount)", new { coupon.ProductName, coupon.Description, coupon.Amount });
        return affected > 0;

    }

    public async Task<bool> UpdateDiscount(Coupon coupon)
    {
        await using var connection = GetPGConnection();
        var affected = await connection.ExecuteAsync("UPDATE Coupon SET Description = @Description, Amount = @Amount WHERE ProductName = @ProductName", new { coupon.Description, coupon.Amount, coupon.ProductName });
        return affected > 0;
    }

    public async Task<bool> DeleteDiscount(string productName)
    {
        await using var connection = GetPGConnection();
        var affected = await connection.ExecuteAsync("DELETE FROM Coupon WHERE ProductName = @ProductName", new { ProductName = productName });
        return affected > 0;
    }
}