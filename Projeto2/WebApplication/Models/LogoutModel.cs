using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication.Models {
    public class LogoutModel {
        [Required]
        public string email { get; set; }
    }
}