using System;
using System.ComponentModel.DataAnnotations;

namespace Bookaholic.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự.")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống.")]
        [StringLength(1000, ErrorMessage = "Nội dung tối đa 1000 ký tự.")]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
