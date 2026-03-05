using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Services.Interfaces
{
    
    public interface IPasswordValidator
    {
        
        PasswordValidationResult Validate(string password);
    }

    public class PasswordValidationResult
    {
        
        public bool IsValid { get; set; }

        public string? ErrorMessage { get; set; }

        public List<string> FailedRequirements { get; set; } = new();

        public static PasswordValidationResult Success()
        {
            return new PasswordValidationResult { IsValid = true };
        }

        public static PasswordValidationResult Failure(List<string> failedRequirements)
        {
            var requirements = failedRequirements ?? new();
            return new PasswordValidationResult
            {
                IsValid = false,
                FailedRequirements = requirements,
                ErrorMessage = "Password does not meet security requirements: " + string.Join(", ", requirements)
            };
        }
    }
}