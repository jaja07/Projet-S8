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
    public class SimulationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SimulationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Simulations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Simulation.Include(s => s.Utilisateur);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Simulations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Simulation == null)
            {
                return NotFound();
            }

            var simulation = await _context.Simulation
                .Include(s => s.Utilisateur)
                .FirstOrDefaultAsync(m => m.IdSimul == id);
            if (simulation == null)
            {
                return NotFound();
            }

            return View(simulation);
        }

        // GET: Simulations/Create
        public IActionResult Create()
        {
            ViewData["UtilisateurId"] = new SelectList(_context.Set<Utilisateur>(), "Id", "Id");
            return View();
        }

        // POST: Simulations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSimul,Date,Duree_simul,UtilisateurId")] Simulation simulation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(simulation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UtilisateurId"] = new SelectList(_context.Set<Utilisateur>(), "Id", "Id", simulation.UtilisateurId);
            return View(simulation);
        }

        // GET: Simulations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Simulation == null)
            {
                return NotFound();
            }

            var simulation = await _context.Simulation.FindAsync(id);
            if (simulation == null)
            {
                return NotFound();
            }
            ViewData["UtilisateurId"] = new SelectList(_context.Set<Utilisateur>(), "Id", "Id", simulation.UtilisateurId);
            return View(simulation);
        }

        // POST: Simulations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSimul,Date,Duree_simul,UtilisateurId")] Simulation simulation)
        {
            if (id != simulation.IdSimul)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(simulation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SimulationExists(simulation.IdSimul))
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
            ViewData["UtilisateurId"] = new SelectList(_context.Set<Utilisateur>(), "Id", "Id", simulation.UtilisateurId);
            return View(simulation);
        }

        // GET: Simulations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Simulation == null)
            {
                return NotFound();
            }

            var simulation = await _context.Simulation
                .Include(s => s.Utilisateur)
                .FirstOrDefaultAsync(m => m.IdSimul == id);
            if (simulation == null)
            {
                return NotFound();
            }

            return View(simulation);
        }

        // POST: Simulations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Simulation == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Simulation'  is null.");
            }
            var simulation = await _context.Simulation.FindAsync(id);
            if (simulation != null)
            {
                _context.Simulation.Remove(simulation);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SimulationExists(int id)
        {
          return (_context.Simulation?.Any(e => e.IdSimul == id)).GetValueOrDefault();
        }
    }
}
