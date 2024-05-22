using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestS8.Data;
using TestS8.Models;

namespace TestS8.Controllers
{
    [Authorize(Roles = "Expert")]
    public class ConnexionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConnexionsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var connexions = await _context.Connexion.ToListAsync();
            return View(connexions);
        }
    }
}
