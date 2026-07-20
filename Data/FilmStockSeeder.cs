using Microsoft.EntityFrameworkCore;
using SakilaApp.Models.Commerce;

namespace SakilaApp.Data;

public static class FilmStockSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sakilaDb = scope.ServiceProvider.GetRequiredService<SakilaContext>();

        if (await appDb.FilmStocks.AnyAsync())
            return;

        var films = await sakilaDb.Films
            .OrderBy(f => f.Title)
            .ToListAsync();

        var random = new Random();

        var stocks = films.Select(f => new FilmStock
        {
            FilmId = f.FilmId,
            Title = f.Title ?? "Sin título",
            UnitPrice = f.RentalRate,
            Stock = random.Next(1, 20),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        appDb.FilmStocks.AddRange(stocks);
        await appDb.SaveChangesAsync();
    }
}
