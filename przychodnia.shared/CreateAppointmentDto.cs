﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przychodnia.Shared
{
    public class CreateAppointmentDto
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public Specialization Specialization { get; set; }
        public string? Symptoms { get; set; }
        public string? Medicines { get; set; }
    }
}
