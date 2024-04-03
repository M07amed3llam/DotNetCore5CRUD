using DotNetCore5CRUD.Models;
using DotNetCore5CRUD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCore5CRUD.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel()
            {
                Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync(),
            };

            return View(viewModel);
        }

    }
}
