using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Views.Customer
{
    public class ReturnCarModel : PageModel
    {
        public int BookingId { get; set; }

        public List<ParkingArea> parkingAreas { get; set; }

    }
}
