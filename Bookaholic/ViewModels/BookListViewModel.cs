using Bookaholic.Models;
using System.Collections.Generic;

namespace Bookaholic.ViewModels
{
    public class BookListViewModel
    {
        public List<Book> Books { get; set; }
        public List<Category> Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string? CurrentCategoryName { get; set; }
    }
}
