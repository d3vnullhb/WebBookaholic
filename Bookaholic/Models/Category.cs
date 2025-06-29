using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Bookaholic.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống!")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
