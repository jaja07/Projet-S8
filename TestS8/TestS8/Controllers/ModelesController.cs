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
    public class ModelesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModelesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Modeles
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Modele.Include(m => m.Simulation);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> IndexSimul(int? id)
        {
            if (id == null || _context.Modele == null)
            {
                return NotFound();
            }

            // Eager loading with Include and Where clause
            var modeles = await _context.Modele
                .Include(m => m.Simulation)
                .Where(m => m.Simulation.SimulationID == id) // Filter by SimulationID
                .ToListAsync();

            if (modeles.Count == 0)
            {
                return NotFound();
            }

            return View(modeles);
        }

        // GET: Modeles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Modele == null)
            {
                return NotFound();
            }

            var modele = await _context.Modele
                .Include(m => m.Simulation)
                .FirstOrDefaultAsync(m => m.ModeleID == id);
            if (modele == null)
            {
                return NotFound();
            }

            return View(modele);
        }

        // GET: Modeles/Create
        public IActionResult Create()
        {
            ViewData["SimulationID"] = new SelectList(_context.Simulation, "SimulationID", "SimulationID");
            return View();
        }

        // POST: Modeles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ModeleID,Nom,Accuracy,Accuracy_cross,Hyperparametre,Duree_simul,SimulationID")] Modele modele)
        {
            if (ModelState.IsValid)
            {
                _context.Add(modele);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SimulationID"] = new SelectList(_context.Simulation, "SimulationID", "SimulationID", modele.SimulationID);
            return View(modele);
        }

        // GET: Modeles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Modele == null)
            {
                return NotFound();
            }

            var modele = await _context.Modele.FindAsync(id);
            if (modele == null)
            {
                return NotFound();
            }
            ViewData["SimulationID"] = new SelectList(_context.Simulation, "SimulationID", "SimulationID", modele.SimulationID);
            return View(modele);
        }

        // POST: Modeles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ModeleID,Nom,Accuracy,Accuracy_cross,Hyperparametre,Duree_simul,SimulationID")] Modele modele)
        {
            if (id != modele.ModeleID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(modele);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModeleExists(modele.ModeleID))
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
            ViewData["SimulationID"] = new SelectList(_context.Simulation, "SimulationID", "SimulationID", modele.SimulationID);
            return View(modele);
        }

        // GET: Modeles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Modele == null)
            {
                return NotFound();
            }

            var modele = await _context.Modele
                .Include(m => m.Simulation)
                .FirstOrDefaultAsync(m => m.ModeleID == id);
            if (modele == null)
            {
                return NotFound();
            }

            return View(modele);
        }

        // POST: Modeles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Modele == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Modele'  is null.");
            }
            var modele = await _context.Modele.FindAsync(id);
            if (modele != null)
            {
                _context.Modele.Remove(modele);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ModeleExists(int id)
        {
          return (_context.Modele?.Any(e => e.ModeleID == id)).GetValueOrDefault();
        }
    }
}
