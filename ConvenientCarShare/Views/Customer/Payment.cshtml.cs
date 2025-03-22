using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConvenientCarShare.Views.Customer
{
    public class PaymentModel : PageModel
    {
        public int CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public List<string> SubmissionErrors = new List<string>();
        public string CreditCardNumber = "";

        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ExpiryDate;

        public string FullName = "";
        public string CVV = "";
        public string ActivationCode = "";
    }
}
