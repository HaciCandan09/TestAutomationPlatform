using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;

namespace TestAutomationPlatform.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Scripts)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError(nameof(category.Name), "Categorie naam is verplicht.");
            }

            if (!ModelState.IsValid)
                return View(category);

            category.Name = category.Name.Trim();
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Categorie '{category.Name}' aangemaakt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            return category == null ? NotFound() : View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError(nameof(category.Name), "Categorie naam is verplicht.");
            }

            if (!ModelState.IsValid)
                return View(category);

            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing == null)
                return NotFound();

            existing.Name = category.Name.Trim();
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Categorie '{existing.Name}' bijgewerkt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Scripts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return RedirectToAction(nameof(Index));

            if (category.Scripts.Any())
            {
                TempData["Error"] = "Categorie kan niet verwijderd worden zolang er scripts aan gekoppeld zijn.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Categorie '{category.Name}' verwijderd.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/api/category")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            return Ok(categories);
        }

        [HttpPost("/api/category")]
        public async Task<IActionResult> CreateApi([FromBody] Category category)
        {
            category.Name = category.Name.Trim();
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }
    }
}