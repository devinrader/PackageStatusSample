using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PackageStatus.Models
{
    public class Package
    {
        [Key]
        public int Id { get; set; }

        public string TrackingNumber { get; set; }
        public string Status { get; set; }
    }
}