using System;

namespace ConvenientCarShare.DataTransferObjects
{
    public class RegistrationDto
    {
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public string Licence { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
