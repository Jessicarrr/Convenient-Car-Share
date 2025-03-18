using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Views.Customer
{
    public class BookModel : PageModel
    {
        public Car Car { get; set; }
        public Booking Booking { get; set; }
        public List<string> Errors = new List<string>();
    }
}
