using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BlindIdea.Application.Services.Interfaces;

namespace BlindIdea.Application.Services.Implementations
{
    
    public class PasswordValidator : IPasswordValidator
    {
        private const int MinimumLength = 8;
        private const string UppercasePattern = @"[A-Z]";
        private const string LowercasePattern = @"[a-z]";
        private const string DigitPattern = @"[0-9]";
        private const string SpecialCharPattern = @"[!@#$%^&*_+=\-\[\]{};:'""\\|,.<>\/?]";

        public PasswordValidationResult Validate(string password)
        {
            if (password == null)
                return PasswordValidationResult.Failure(new List<string> { "Password is required" });

            var failedRequirements = new List<string>();

            if (password.Length < MinimumLength)
            {
                failedRequirements.Add($"At least {MinimumLength} characters");
            }

            if (!Regex.IsMatch(password, UppercasePattern))
            {
                failedRequirements.Add("At least one uppercase letter (A-Z)");
            }

            if (!Regex.IsMatch(password, LowercasePattern))
            {
                failedRequirements.Add("At least one lowercase letter (a-z)");
            }

            if (!Regex.IsMatch(password, DigitPattern))
            {
                failedRequirements.Add("At least one number (0-9)");
            }

            if (!Regex.IsMatch(password, SpecialCharPattern))
            {
                failedRequirements.Add("At least one special character (!@#$%^&*_+=-[]{}';:\"\\|,.<>/?");
            }

            if (failedRequirements.Count > 0)
            {
                return PasswordValidationResult.Failure(failedRequirements);
            }

            return PasswordValidationResult.Success();
        }
    }
}