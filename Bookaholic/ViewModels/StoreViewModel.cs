using Bookaholic.Models;
using System.Collections.Generic;

namespace Bookaholic.ViewModels
{
    public class StoreViewModel
    {
        public List<Book> Books { get; set; }
        public List<Author> Authors { get; set; }
    }
}
