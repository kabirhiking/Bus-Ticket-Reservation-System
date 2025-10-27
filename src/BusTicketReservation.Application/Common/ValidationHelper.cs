using System.ComponentModel.DataAnnotations;

namespace BusTicketReservation.Application.Common;

public static class ValidationHelper
{
    public static List<string> ValidateObject<T>(T obj) where T : class
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(obj);
        
        Validator.TryValidateObject(obj, context, validationResults, true);
        
        return validationResults.Select(vr => vr.ErrorMessage ?? "Unknown validation error").ToList();
    }
    
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    public static bool IsValidMobileNumber(string mobileNumber)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
            return false;
            
        // Remove all non-digit characters
        var digits = new string(mobileNumber.Where(char.IsDigit).ToArray());
        
        // Check if it has 10-15 digits (international format)
        return digits.Length >= 10 && digits.Length <= 15;
    }
    
    public static bool IsValidDate(DateTime date)
    {
        return date >= DateTime.Today;
    }
    
    public static bool IsValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}