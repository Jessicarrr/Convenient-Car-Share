using System;
using System.ComponentModel.DataAnnotations;

namespace ConvenientCarShare.Attributes
{
    public class MinimumAge : ValidationAttribute
    {
        public int Minimum { get; set; }

        public MinimumAge(int minimum)
        {
            this.Minimum = minimum;
        }

        protected override ValidationResult IsValid(object obj, ValidationContext context)
        {
            if (obj == null)
            {
                return new ValidationResult("You are required to enter a valid date.");
            }

            DateTime date;

            try
            {
                date = Convert.ToDateTime(obj);
            }
            catch(Exception)
            {
                return new ValidationResult("You are required to enter a valid date.");
            }
            

            bool result = date <= DateTime.Now.AddYears(-1 * Minimum);

            if (result == true)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"You are required to be at least {Minimum} years old.");
        }
    }
}
