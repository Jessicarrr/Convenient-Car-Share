using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Models
{
    public class ParkingArea
    {
        public int Id { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public int MaximumCars { get; set; }

        public ParkingArea(double Latitude, double Longitude, int MaximumCars)
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.MaximumCars = MaximumCars;
        }
    }
}
