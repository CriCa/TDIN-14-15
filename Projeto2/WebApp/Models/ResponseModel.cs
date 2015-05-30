using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ResponseModel {
        [Required]
        public string State { get; set; }

        [Required]
        public string Message { get; set; }
    }
}