using ConvenientCarShare.Areas.Identity.Data;
using System.Collections.Generic;

namespace ConvenientCarShare.DataTransferObjects
{
    public class RegistrationResult
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
        // Return the created user and the confirmation token if successful.
        public ApplicationUser User { get; set; }
        public string EmailConfirmationToken { get; set; }
    }
}
