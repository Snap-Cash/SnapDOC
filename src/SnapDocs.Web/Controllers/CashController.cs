using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapDocs.Domain.Entities;
using SnapDocs.Domain.Enums;
using SnapDocs.Infrastructure.Persistence;
using SnapDocs.Web.Models;

namespace SnapDocs.Web.Controllers;

public class CashController : Controller
{
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly SnapDocsDbContext _db;

    public CashController(SnapDocsDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, Guid? accountId)
    {
        var start = from?.Date ?? DateTime.Today.AddDays(-30);
        var end = to?.Date ?? DateTime.Today;

        var accounts = await _db.CashAccounts
            .Where(x => x.CompanyId == DemoCompanyId && x.IsActive)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var txQuery = _db.CashTransactions
            .Include(x => x.CashAccount)
            .Include(x => x.Customer)
            .Include(x => x.Document)
            .Where(x => x.CompanyId == DemoCompanyId && x.TransactionDate >= start && x.TransactionDate <= end);

        if (accountId.HasValue)
        {
            txQuery = txQuery.Where(x => x.CashAccountId == accountId.Value);
        }

        var transactions = await txQuery
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(150)
            .ToListAsync();

        var accountBalances = new List<CashAccountBalanceViewModel>();
        foreach (var account in accounts)
        {
            var accountTx = await _db.CashTransactions
                .Where(x => x.CompanyId == DemoCompanyId && x.CashAccountId == account.Id)
                .ToListAsync();

            accountBalances.Add(new CashAccountBalanceViewModel
            {
                Account = account,
                Receipts = accountTx.Sum(x => x.Debit),
                Payments = accountTx.Sum(x => x.Credit)
            });
        }

        var today = DateTime.Today;
        var model = new CashDashboardViewModel
        {
            Accounts = accountBalances,
            TotalCashBalance = accountBalances.Sum(x => x.Balance),
            TodayReceipts = await _db.CashTransactions.Where(x => x.CompanyId == DemoCompanyId && x.TransactionDate == today).SumAsync(x => x.Debit),
            TodayPayments = await _db.CashTransactions.Where(x => x.CompanyId == DemoCompanyId && x.TransactionDate == today).SumAsync(x => x.Credit),
            RecentTransactions = transactions
        };

        ViewBag.From = start.ToString("yyyy-MM-dd");
        ViewBag.To = end.ToString("yyyy-MM-dd");
        ViewBag.AccountId = accountId;
        return View(model);
    }

    public IActionResult CreateAccount()
    {
        return View(new CashAccountCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount(CashAccountCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (model.IsDefault)
        {
            var defaults = await _db.CashAccounts.Where(x => x.CompanyId == DemoCompanyId && x.IsDefault).ToListAsync();
            foreach (var account in defaults) account.IsDefault = false;
        }

        var account = new CashAccount
        {
            CompanyId = DemoCompanyId,
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Type = model.Type,
            CurrencyCode = model.CurrencyCode,
            OpeningBalance = model.OpeningBalance,
            IsDefault = model.IsDefault,
            IsActive = true
        };

        _db.CashAccounts.Add(account);

        if (model.OpeningBalance != 0)
        {
            _db.CashTransactions.Add(new CashTransaction
            {
                CompanyId = DemoCompanyId,
                CashAccount = account,
                Type = CashTransactionType.OpeningBalance,
                TransactionDate = DateTime.Today,
                Debit = model.OpeningBalance > 0 ? model.OpeningBalance : 0,
                Credit = model.OpeningBalance < 0 ? Math.Abs(model.OpeningBalance) : 0,
                Description = "رصيد افتتاحي"
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "تم إنشاء حساب الخزنة بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
