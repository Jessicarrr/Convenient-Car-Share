using ConvenientCarShare.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ConvenientCarShare.Models
{
    public class Message
    {
        public int ID { get; set; }

        [Required]
        public bool HasBeenRead { get; set; } = false;

        [Required]
        public string Title { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public virtual ApplicationUser Sender { get; set; }

        [Required]
        public virtual ApplicationUser Receiver { get; set; }

        [Required]
        public DateTime SentDateTime { get; set; }

        public virtual Message ReplyTo { get; set; }

        public bool IsBroadcastMessage { get; set; }
    }
}
