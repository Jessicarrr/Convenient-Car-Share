using ConvenientCarShare.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime ExtensionDate { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal ExtensionPrice { get; set; }

        public string ActivationCode { get; set; }
        
        public string Status { get; set; }

        public DateTime ReturnDate { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public virtual Car Car { get; set; }

        public virtual ParkingArea ReturnArea { get; set; }
    }
}
