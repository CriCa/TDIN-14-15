using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication.Models {
    public class CreateModel {
        [Required]
        public long client_id { get; set; }

        [Required]
        public long book_id { get; set; }

        [Required]
        public int quantity { get; set; }

        [Required]
        public string address { get; set; }
        
        public List<BookModel> books { get; set; }

        public class BookModel
        {
            public long id { get; set; }
            public string title { get; set; }
            public long quantity { get; set; }
            public double price { get; set; }
        }
    }
}