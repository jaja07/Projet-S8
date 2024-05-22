using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestS8.Data;
using TestS8.Models;

namespace TestS8.Controllers
{
    public class ParametresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ParametresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Parametres
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Parametres.Include(p => p.Modeles);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Parametres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Parametres == null)
            {
                return NotFound();
            }

            var parametres = await _context.Parametres
                .Include(p => p.Modeles)
                .FirstOrDefaultAsync(m => m.ParametresID == id);
            if (parametres == null)
            {
                return NotFound();
            }

            return View(parametres);
        }

        // GET: Parametres/Create
        public IActionResult Create()
        {
            ViewData["ModeleID"] = new SelectList(_context.Modele, "ModeleID", "ModeleID");
            return View();
        }

        // POST: Parametres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParametresID,Nom,Valeur,ModeleID")] Parametres parametres)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parametres);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ModeleID"] = new SelectList(_context.Modele, "ModeleID", "ModeleID", parametres.ModeleID);
            return View(parametres);
        }

        // GET: Parametres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Parametres == null)
            {
                return NotFound();
            }

            var parametres = await _context.Parametres.FindAsync(id);
            if (parametres == null)
            {
                return NotFound();
            }
            ViewData["ModeleID"] = new SelectList(_context.Modele, "ModeleID", "ModeleID", parametres.ModeleID);
            return View(parametres);
        }

        // POST: Parametres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParametresID,Nom,Valeur,ModeleID")] Parametres parametres)
        {
            if (id != parametres.ParametresID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parametres);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParametresExists(parametres.ParametresID))
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
            ViewData["ModeleID"] = new SelectList(_context.Modele, "ModeleID", "ModeleID", parametres.ModeleID);
            return View(parametres);
        }

        // GET: Parametres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Parametres == null)
            {
                return NotFound();
            }

            var parametres = await _context.Parametres
                .Include(p => p.Modeles)
                .FirstOrDefaultAsync(m => m.ParametresID == id);
            if (parametres == null)
            {
                return NotFound();
            }

            return View(parametres);
        }

        // POST: Parametres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Parametres == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Parametres'  is null.");
            }
            var parametres = await _context.Parametres.FindAsync(id);
            if (parametres != null)
            {
                _context.Parametres.Remove(parametres);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParametresExists(int id)
        {
          return (_context.Parametres?.Any(e => e.ParametresID == id)).GetValueOrDefault();
        }
    }
}
