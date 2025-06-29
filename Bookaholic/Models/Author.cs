using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookaholic.Models
{
    public class Author
    {
        [Key]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Tên tác giả không được để trống")]
        [StringLength(100, ErrorMessage = "Tên tác giả tối đa 100 ký tự")]
        [Display(Name = "Tên tác giả")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Đường dẫn ảnh tối đa 255 ký tự")]
        [Display(Name = "Ảnh tác giả")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Tiểu sử")]
        [StringLength(1000, ErrorMessage = "Tiểu sử tối đa 1000 ký tự")]
        public string? Bio { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
