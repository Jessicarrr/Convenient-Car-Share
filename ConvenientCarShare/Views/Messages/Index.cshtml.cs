using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConvenientCarShare.Views.Messages
{
    public class IndexModel : PageModel
    {
        public List<List<Message>> MessagesAndReplies = new List<List<Message>>();
        public bool IsAdmin = false;
        public ApplicationUser CurrentUser { get; set; }
        public string PageMessage { get; set; }
        public List<string> Errors = new List<string>();
    }
}