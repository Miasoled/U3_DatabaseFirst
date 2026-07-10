using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SakilaApp.Data;
using SakilaApp.Models;

namespace SakilaApp.Controllers
{
    public class FilmCategoriesController : Controller
    {
        private readonly SakilaContext _context;

        public FilmCategoriesController(SakilaContext context)
        {
            _context = context;
        }

        // GET: FilmCategories
        public async Task<IActionResult> Index()
        {
            var sakilaContext = _context.FilmCategories.Include(f => f.Category).Include(f => f.Film);
            return View(await sakilaContext.ToListAsync());
        }

        // GET: FilmCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filmCategory = await _context.FilmCategories
                .Include(f => f.Category)
                .Include(f => f.Film)
                .FirstOrDefaultAsync(m => m.FilmId == id);
            if (filmCategory == null)
            {
                return NotFound();
            }

            return View(filmCategory);
        }

        // GET: FilmCategories/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
            ViewData["FilmId"] = new SelectList(_context.Films, "FilmId", "FilmId");
            return View();
        }

        // POST: FilmCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FilmId,CategoryId,LastUpdate")] FilmCategory filmCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(filmCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", filmCategory.CategoryId);
            ViewData["FilmId"] = new SelectList(_context.Films, "FilmId", "FilmId", filmCategory.FilmId);
            return View(filmCategory);
        }

        // GET: FilmCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filmCategory = await _context.FilmCategories.FindAsync(id);
            if (filmCategory == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", filmCategory.CategoryId);
            ViewData["FilmId"] = new SelectList(_context.Films, "FilmId", "FilmId", filmCategory.FilmId);
            return View(filmCategory);
        }

        // POST: FilmCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FilmId,CategoryId,LastUpdate")] FilmCategory filmCategory)
        {
            if (id != filmCategory.FilmId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(filmCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmCategoryExists(filmCategory.FilmId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", filmCategory.CategoryId);
            ViewData["FilmId"] = new SelectList(_context.Films, "FilmId", "FilmId", filmCategory.FilmId);
            return View(filmCategory);
        }

        // GET: FilmCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filmCategory = await _context.FilmCategories
                .Include(f => f.Category)
                .Include(f => f.Film)
                .FirstOrDefaultAsync(m => m.FilmId == id);
            if (filmCategory == null)
            {
                return NotFound();
            }

            return View(filmCategory);
        }

        // POST: FilmCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var filmCategory = await _context.FilmCategories.FindAsync(id);
            if (filmCategory != null)
            {
                _context.FilmCategories.Remove(filmCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilmCategoryExists(int id)
        {
            return _context.FilmCategories.Any(e => e.FilmId == id);
        }
    }
}
