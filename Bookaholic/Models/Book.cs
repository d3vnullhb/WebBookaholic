using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookaholic.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Tên sách là bắt buộc.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn tác giả.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn tác giả.")]
        public int? AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public Author? Author { get; set; }

        [StringLength(100)]
        public string? Translator { get; set; }

        [StringLength(100)]
        public string? Publisher { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải lớn hơn 0.")]
        public int? Pages { get; set; }

        [Range(1900, 2100, ErrorMessage = "Năm xuất bản không hợp lệ.")]
        public int? PublishYear { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn 0.")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Giá gốc là bắt buộc.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá gốc phải lớn hơn 0.")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Số lượng tồn là bắt buộc.")]      
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }


        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thể loại.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn thể loại.")]
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
        public ICollection<BookReview>? BookReviews { get; set; }
    }
}
