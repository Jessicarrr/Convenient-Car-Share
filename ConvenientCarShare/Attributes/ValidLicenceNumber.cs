using System;
using System.ComponentModel.DataAnnotations;

namespace ConvenientCarShare.Attributes
{
    public class ValidLicenceNumber : ValidationAttribute
    {
        private string GenericError = "You must enter a licence number between 8 and 10 characters long.";

        protected override ValidationResult IsValid(object obj, ValidationContext context)
        {
            if (obj == null)
            {
                return ValidationResult.Success;
            }

            string input = obj as string;

            if (string.IsNullOrEmpty(input))
            {
                return ValidationResult.Success;
            }

            if (input.Length >= 8 && input.Length <= 10)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(GenericError);
        }
    }
}
