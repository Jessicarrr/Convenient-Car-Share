using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Views.Customer
{
    public class ManageTrips : PageModel
    {
        public Booking[] bookings { get; set; }
        public List<string> messages = new List<string>();
        public List<string> errors = new List<string>();
    }
}
