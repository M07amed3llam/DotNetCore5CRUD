using DotNetCore5CRUD.Models;
using DotNetCore5CRUD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCore5CRUD.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly List<string> _allowedExtesion = new List<string> { ".jpg", ".png" };
        private long _MaxAllowedPosterSize = 1048576;
        private readonly IToastNotification _toastNotification;

        public MoviesController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
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

            return View("MovieForm", viewModel);
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
                return View("MovieForm", model);
            }

            var files = Request.Form.Files; // Bring all files 
            if (!files.Any())
            {
                model.Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync();
                ModelState.AddModelError("Poster", "Please select movie poster!");
                return View("MovieForm", model);
            }

            var poster = files.FirstOrDefault();

            // validate image extension
            if (!_allowedExtesion.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                model.Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync();
                ModelState.AddModelError("Poster", "Only .PNG, .JPG images are allowed!");
                return View("MovieForm", model);
            }

            // validate image size
            if(poster.Length > _MaxAllowedPosterSize)
            {
                model.Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync();
                ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                return View("MovieForm", model);
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

            // Add Toaster Notification
            _toastNotification.AddSuccessToastMessage("Movie added successfully");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);
            
            if(movie == null)
                return NotFound();

            var viewModel = new MovieFormViewModel()
            {
                Id = movie.Id,
                Title = movie.Title,
                GenreId = movie.GenreId,
                Rate = movie.Rate,
                Year= movie.Year,
                StoreLine= movie.StoreLine,
                Poster= movie.Poster,
                Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync()
            };

            return View("MovieForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context
                                .Genres
                                .OrderBy(m => m.Name)
                                .ToListAsync();
                return View("MovieForm", model);
            }

            var movie = await _context.Movies.FindAsync(model.Id);

            if (movie == null)
                return NotFound();

            var files = Request.Form.Files;
            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                
                using var dataStream = new MemoryStream();
                
                await poster.CopyToAsync(dataStream);

                model.Poster = dataStream.ToArray();

                // validate image extension
                if (!_allowedExtesion.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    model.Genres = await _context
                                    .Genres
                                    .OrderBy(m => m.Name)
                                    .ToListAsync();
                    ModelState.AddModelError("Poster", "Only .PNG, .JPG images are allowed!");
                    return View("MovieForm", model);
                }

                // validate image size
                if (poster.Length > _MaxAllowedPosterSize)
                {
                    model.Genres = await _context
                                    .Genres
                                    .OrderBy(m => m.Name)
                                    .ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                    return View("MovieForm", model);
                }

                movie.Poster = dataStream.ToArray();
            }

            movie.Title = model.Title;
            movie.GenreId = model.GenreId;
            movie.Year = model.Year;
            movie.Rate = model.Rate;
            movie.StoreLine = model.StoreLine;

            _context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie Updated successfully");

            return RedirectToAction(nameof(Index));
        }
    }
}
