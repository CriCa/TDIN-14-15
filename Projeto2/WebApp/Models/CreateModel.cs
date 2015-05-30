using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CreateModel {
        [Required]
        public long user_id { get; set; }

        [Required]
        public string title { get; set; }

        [Required]
        public string description { get; set; }

        public string category_id { get; set; }

        public List<Category> categories { get; set; }

        public class Category {
            public long id;
            public string name;
        }
    }
}