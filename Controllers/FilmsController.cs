using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SakilaApp.Data;
using SakilaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace SakilaApp.Controllers
{
    
    public class FilmsController : Controller
    {
        private readonly SakilaContext _context;

        public FilmsController(SakilaContext context)
        {
            _context = context;
        }

        // GET: Films
        // public async Task<IActionResult> Index()
        // {
        //     var sakilaContext = _context.Films.Include(f => f.Language).Include(f => f.OriginalLanguage);
            
        //     string palabra = "drama";
        //     var films = await sakilaContext 
        //         .Where(f => EF.Functions.ILike(f.Description, $"%{palabra}%"))
        //         .OrderBy(f => f.Length)
        //         .ToListAsync();
        //     return View(films);
        // }

        /*public async Task<IActionResult> Index()
        {
            var peliculas = await _context.Films
                .Where(f => f.Title.StartsWith("A"))
                .ToListAsync();

            return View(peliculas);
        }*/

//         public async Task<IActionResult> Index()
// {
//     var peliculas = await _context.Films
//         .Where(f => f.ReplacementCost > 25.50m)
//         .ToListAsync();

//     return View(peliculas);
// }
        
        
        // public async Task<IActionResult> Index()
        // {
        //     var peliculas = await _context.Films
        //         .Where(f => f.ReplacementCost > 25.50m)
        //         .ToListAsync();

        //     return View(peliculas);
        // }
        
        /*public async Task<IActionResult> Index()
        {
            var peliculas = await _context.Films
                .Include(f => f.Language)
                .Include(f => f.OriginalLanguage)
                .OrderBy(f => f.Title)
                .ToListAsync(); 

            return View(peliculas);
        }*/


            /*public async Task<IActionResult> Index()
            {
                var peliculas = await _context.Films
                    .Join(_context.FilmCategories,
                        film => film.FilmId,
                        filmCategory => filmCategory.FilmId,
                        (film, filmCategory) => new { film, filmCategory })
                    .Join(_context.Categories,
                        temp => temp.filmCategory.CategoryId,
                        category => category.CategoryId,
                        (temp, category) => new { temp.film, category })
                    .Where(x => x.category.Name == "Drama")
                    .OrderByDescending(x => x.film.Length)
                    .Take(10)
                    .Select(x => x.film)
                    .ToListAsync();

                return View(peliculas);
            }*/

        /*public async Task<IActionResult> Index()
        {
            //con tres join
            var peliculas = await _context.Films
                .Join(_context.Languages,
                    film => film.LanguageId,
                    language => language.LanguageId,
                    (film, language) => new { film, language })
                .Join(_context.FilmCategories,
                    temp => temp.film.FilmId,
                    filmCategory => filmCategory.FilmId,
                    (temp, filmCategory) => new { temp.film, temp.language, filmCategory })
                .Join(_context.Categories,
                    temp => temp.filmCategory.CategoryId,
                    category => category.CategoryId,
                    (temp, category) => new { temp.film, temp.language, category })
                .Where(x => x.language.Name == "English" && x.category.Name == "Comedy")
                .OrderBy(x => x.film.Title)
                .Select(x => x.film)
                .ToListAsync();

            return View(peliculas);
        }*/

        /*public async Task<IActionResult> Index()
        {
            var peliculas = await _context.Films
                .Join(_context.Languages,
                    f => f.LanguageId,
                    l => l.LanguageId,
                    (f, l) => new { f, l })
                .Where(x => x.l.Name == "English"
                        && x.f.Title.StartsWith("A"))
                .OrderByDescending(x => x.f.ReplacementCost)
                .Take(5)
                .Select(x => x.f)
                .ToListAsync();

            return View(peliculas);
        }*/
        //esto es lo nuevo 
        /*public async Task<IActionResult> Index(string? buscar, int? duracionMinima)
        {
            var consulta = _context.Films.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(f => f.Title.Contains(buscar));
            }

            if (duracionMinima.HasValue)
            {
                consulta = consulta.Where(f => f.Length >= duracionMinima.Value);
            }

            var peliculas = await consulta
                .OrderBy(f => f.Title)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.DuracionMinima = duracionMinima;

            return View(peliculas);
        }*/

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index(
            string? buscar,
            int? duracionMinima,
            int pagina = 1)
        {
            int tamanioPagina = 10;

            var consulta = _context.Films.AsQueryable(); // significa que todavia no termina la consulta 

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(f => f.Title.Contains(buscar));
            }

            if (duracionMinima.HasValue)
            {
                consulta = consulta.Where(f => f.Length >= duracionMinima.Value);
            }

            int totalRegistros = await consulta.CountAsync();

            var peliculas = await consulta
                .OrderBy(f => f.Title)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.DuracionMinima = duracionMinima;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanioPagina);

            return View(peliculas);
        }

         // GET: Films/Details/5
         [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();  
            }

            var film = await _context.Films
                .Include(f => f.Language)
                .Include(f => f.OriginalLanguage)
                .FirstOrDefaultAsync(m => m.FilmId == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        // GET: Films/Create
        public IActionResult Create()
        {
            ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId");
            ViewData["OriginalLanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId");
            return View();
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FilmId,Title,Description,ReleaseYear,LanguageId,OriginalLanguageId,RentalDuration,RentalRate,Length,ReplacementCost,LastUpdate,SpecialFeatures,Fulltext")] Film film)
        {
            if (ModelState.IsValid)
            {
                _context.Add(film);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId", film.LanguageId);
            ViewData["OriginalLanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId", film.OriginalLanguageId);
            return View(film);
        }

        // GET: Films/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }
            ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId", film.LanguageId);
            ViewData["OriginalLanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId", film.OriginalLanguageId);
            return View(film);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FilmId,Title,Description,ReleaseYear,LanguageId,OriginalLanguageId,RentalDuration,RentalRate,Length,ReplacementCost,LastUpdate,SpecialFeatures,Fulltext")] Film film)
        {
            if (id != film.FilmId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(film);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmExists(film.FilmId))
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
            ViewData["LanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId", film.LanguageId);
            ViewData["OriginalLanguageId"] = new SelectList(_context.Languages, "LanguageId", "LanguageId", film.OriginalLanguageId);
            return View(film);
        }

        // GET: Films/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .Include(f => f.Language)
                .Include(f => f.OriginalLanguage)
                .FirstOrDefaultAsync(m => m.FilmId == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var film = await _context.Films.FindAsync(id);
            if (film != null)
            {
                _context.Films.Remove(film);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilmExists(int id)
        {
            return _context.Films.Any(e => e.FilmId == id);
        }
    }
}
