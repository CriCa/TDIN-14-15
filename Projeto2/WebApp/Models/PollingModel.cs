﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class PollingModel {
        [Required]
        public int id { get; set; }

        [Required]
        public int max_time { get; set; }
    }
}