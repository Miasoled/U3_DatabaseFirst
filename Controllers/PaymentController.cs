using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SakilaApp.Data;
using SakilaApp.Models.Commerce;
using SakilaApp.Services.Payments;

namespace SakilaApp.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PayPhoneApiLinkService _payPhoneService;

    public PaymentController(ApplicationDbContext context, PayPhoneApiLinkService payPhoneService)
    {
        _context = context;
        _payPhoneService = payPhoneService;
    }

    public async Task<IActionResult> CreateLink(int orderId)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.PurchaseOrderId == orderId);

        if (order == null) return NotFound();

        string clientTransactionId = DateTime.Now.ToString("yyMMddHHmmssfff")[..15];
        string reference = $"Orden Sakila #{order.PurchaseOrderId}";

        string link = await _payPhoneService.CreatePaymentLinkAsync(
            order.Total,
            clientTransactionId,
            reference);

        var payment = new PaymentTransaction
        {
            PurchaseOrderId = order.PurchaseOrderId,
            ClientTransactionId = clientTransactionId,
            PayphonePaymentUrl = link,
            AmountInCents = (int)Math.Round(order.Total * 100, MidpointRounding.AwayFromZero),
            Status = "Pending"
        };

        _context.PaymentTransactions.Add(payment);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = payment.PaymentTransactionId });
    }

    public async Task<IActionResult> Details(int id)
    {
        var payment = await _context.PaymentTransactions
            .Include(p => p.PurchaseOrder)
            .ThenInclude(o => o.Details)
            .FirstOrDefaultAsync(p => p.PaymentTransactionId == id);

        if (payment == null) return NotFound();
        return View(payment);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var payment = await _context.PaymentTransactions
            .Include(p => p.PurchaseOrder)
            .ThenInclude(o => o.Details)
            .FirstOrDefaultAsync(p => p.PaymentTransactionId == id);

        if (payment == null) return NotFound();

        payment.Status = "Paid";
        payment.ConfirmedAt = DateTime.UtcNow;
        payment.PurchaseOrder.Status = "Paid";

        foreach (var detail in payment.PurchaseOrder.Details)
        {
            var stock = await _context.FilmStocks.FindAsync(detail.FilmStockId);
            if (stock != null)
            {
                stock.Stock -= detail.Quantity;
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }
}