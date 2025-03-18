using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Views.Messages
{
    public class ComposeModel : PageModel
    {
        public Message Message = new Message();
        public List<string> Errors = new List<string>();

        public string Receiver = "";
        public string Title = "";
        public string Text = "";
    }
}
