using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Views.Customer
{
    public class IndexModel : PageModel
    {
        public Dictionary<ParkingArea, List<Car>> ParkingInfo { get; set; }
        public bool HasBookingsWithoutReturns = false;
    }
}
