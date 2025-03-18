using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required]
        public string Brand { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public string Colour { get; set; }

        [Required]
        public string NumberPlate { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Capacity { get; set; }


        public virtual ParkingArea CurrentlyParkedAt { get; set; }

        public Car(string Brand, string Model, string Colour, string NumberPlate, double Latitude, double Longitude, decimal Price, string Capacity)
        {
            this.Brand = Brand;
            this.Model = Model;
            this.Colour = Colour;
            this.NumberPlate = NumberPlate;
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.Price = Price;
            this.Capacity = Capacity;
        }

        public Car(string Brand, string Model, string Colour, string NumberPlate, double Latitude, double Longitude, decimal Price, string Capacity, ParkingArea CurrentlyParkedAt)
        {
            this.Brand = Brand;
            this.Model = Model;
            this.Colour = Colour;
            this.NumberPlate = NumberPlate;
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.Price = Price;
            this.Capacity = Capacity;
            this.CurrentlyParkedAt = CurrentlyParkedAt;

        }
    }
}
