using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SakilaApp.Data;
using SakilaApp.Models.Commerce;

namespace SakilaApp.Controllers;

[Authorize]
public class StoreController : Controller
{
    private readonly ApplicationDbContext _context;

    public StoreController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var productos = await _context.FilmStocks
            .Where(f => f.IsActive && f.Stock > 0)
            .OrderBy(f => f.Title)
            .ToListAsync();

        return View(productos);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int filmStockId, int quantity)
    {
        if (quantity <= 0) quantity = 1;

        var userEmail = User.Identity?.Name ?? "usuario@local";
        var stock = await _context.FilmStocks.FindAsync(filmStockId);
        if (stock == null) return NotFound();

        if (quantity > stock.Stock)
        {
            TempData["Error"] = "No existe stock suficiente.";
            return RedirectToAction(nameof(Index));
        }

        var item = await _context.ShoppingCartItems
            .FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.FilmStockId == filmStockId);

        if (item == null)
        {
            _context.ShoppingCartItems.Add(new ShoppingCartItem
            {
                UserEmail = userEmail,
                FilmStockId = filmStockId,
                Quantity = quantity
            });
        }
        else
        {
            item.Quantity += quantity;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Cart));
    }

    public async Task<IActionResult> Cart()
    {
        var userEmail = User.Identity?.Name ?? "usuario@local";

        var items = await _context.ShoppingCartItems
            .Include(c => c.FilmStock)
            .Where(c => c.UserEmail == userEmail)
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        var userEmail = User.Identity?.Name ?? "usuario@local";

        var cartItems = await _context.ShoppingCartItems
            .Include(c => c.FilmStock)
            .Where(c => c.UserEmail == userEmail)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["Error"] = "El carrito está vacío.";
            return RedirectToAction(nameof(Cart));
        }

        foreach (var item in cartItems)
        {
            if (item.Quantity > item.FilmStock.Stock)
            {
                TempData["Error"] = $"Stock insuficiente para {item.FilmStock.Title}.";
                return RedirectToAction(nameof(Cart));
            }
        }

        var order = new PurchaseOrder
        {
            UserEmail = userEmail,
            Status = "Pending"
        };

        foreach (var item in cartItems)
        {
            var subtotal = item.Quantity * item.FilmStock.UnitPrice;

            order.Details.Add(new PurchaseOrderDetail
            {
                FilmStockId = item.FilmStockId,
                FilmTitle = item.FilmStock.Title,
                Quantity = item.Quantity,
                UnitPrice = item.FilmStock.UnitPrice,
                Subtotal = subtotal
            });

            order.Total += subtotal;
        }

        _context.PurchaseOrders.Add(order);
        _context.ShoppingCartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return RedirectToAction("CreateLink", "Payment", new { orderId = order.PurchaseOrderId });
    }
}