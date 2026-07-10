using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SakilaApp.Data;
using SakilaApp.Models.Commerce;

namespace SakilaApp.Controllers;

[Authorize(Roles = "Administrador")]
public class InventoryController : Controller
{
    private readonly SakilaContext _sakilaContext;
    private readonly ApplicationDbContext _appContext;

    public InventoryController(SakilaContext sakilaContext, ApplicationDbContext appContext)
    {
        _sakilaContext = sakilaContext;
        _appContext = appContext;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _appContext.FilmStocks
            .OrderBy(f => f.Title)
            .ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> Inicializar()
    {
        if (await _appContext.FilmStocks.AnyAsync())
        {
            return RedirectToAction(nameof(Index));
        }

        var films = await _sakilaContext.Films
            .OrderBy(f => f.Title)
            .Take(20)
            .ToListAsync();

        foreach (var film in films)
        {
            _appContext.FilmStocks.Add(new FilmStock
            {
                FilmId = film.FilmId,
                Title = film.Title,
                UnitPrice = film.RentalRate,
                Stock = 10,
                IsActive = true
            });
        }

        await _appContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AumentarStock(int id, int cantidad)
    {
        var item = await _appContext.FilmStocks.FindAsync(id);
        if (item == null) return NotFound();

        item.Stock += cantidad;
        await _appContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}