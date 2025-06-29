using System;
using System.ComponentModel.DataAnnotations;

namespace Bookaholic.Models
{
    public class Subscriber
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        public DateTime SubscribedAt { get; set; } = DateTime.Now;
    }
}
