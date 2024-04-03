using DotNetCore5CRUD.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetCore5CRUD.ViewModels
{
    public class MovieFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string Title { get; set; }
        
        public int Year { get; set; }
        [Range(1, 10)]
        
        public double Rate { get; set; }
        [Required, MaxLength(2500)]
        
        public string StoreLine { get; set; }

        [Display(Name = "--Select poster--")]
        public byte[] Poster { get; set; }

        [Display(Name = "Genre")]
        public byte GenreId { get; set; }

        public IEnumerable<Genre> Genres { get; set; }

    }
}
