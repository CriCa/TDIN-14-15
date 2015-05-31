using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication.Models {
    public class LoginResponseModel : ResponseModel {
        [Required]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}