using DotNetCore5CRUD.Models;
using DotNetCore5CRUD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync();
                return View(model);
            }

            var files = Request.Form.Files; // Bring all files 
            if (!files.Any())
            {
                model.Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync();
                ModelState.AddModelError("Poster", "Please select movie poster!");
                return View(model);
            }

            var poster = files.FirstOrDefault();
            var allowedExtesion = new List<string> { ".jpg", ".png" };

            // validate image extension
            if (!allowedExtesion.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                model.Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync();
                ModelState.AddModelError("Poster", "Only .PNG, .JPG images are allowed!");
                return View(model);
            }

            // validate image size
            if(poster.Length > 1048576)
            {
                model.Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync();
                ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                return View(model);
            }

            using var dataStream = new MemoryStream();
            await poster.CopyToAsync(dataStream);

            // map values to movie model
            var movie = new Movie
            {
                Title = model.Title,
                GenreId = model.GenreId,
                Year = model.Year,
                Rate = model.Rate,
                StoreLine = model.StoreLine,
                Poster = dataStream.ToArray()
            };

            _context.Movies.Add(movie);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}
