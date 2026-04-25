using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NfaSporSalonu.Models;
using System.Diagnostics;

namespace NfaSporSalonu.Controllers
{
    public class HomeController : Controller
    {
        private readonly NfaSporSalonuDbContext _context;

        public HomeController(NfaSporSalonuDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Paketler()
        {
            var plans = await _context.MembershipPlans
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Price)
                .ToListAsync();

            return View(plans);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
