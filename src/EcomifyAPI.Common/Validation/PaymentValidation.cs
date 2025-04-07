using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Validation;

public class PaymentValidation
{
    public static IReadOnlyList<ValidationError> ValidateCreditCard(string cardNumber, string expirationDate, string cvv)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            errors.Add(ValidationError.Create("Card number is required", "ERR_CARD_NUMBER_REQUIRED", "CardNumber"));
        }

        if (string.IsNullOrWhiteSpace(expirationDate))
        {
            errors.Add(ValidationError.Create("Expiration date is required", "ERR_EXP_DATE_REQUIRED", "ExpirationDate"));
        }

        if (string.IsNullOrWhiteSpace(cvv))
        {
            errors.Add(ValidationError.Create("CVV is required", "ERR_CVV_REQUIRED", "CVV"));
        }

        if (!IsValidCreditCardNumber(cardNumber))
        {
            errors.Add(ValidationError.Create("Invalid card number", "ERR_INVALID_CARD_NUMBER", "CardNumber"));
        }

        if (!IsValidExpirationDate(expirationDate))
        {
            errors.Add(ValidationError.Create("Invalid expiration date", "ERR_INVALID_EXP_DATE", "ExpirationDate"));
        }

        return errors;
    }

    private static bool IsValidCreditCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return false;
        }

        int sum = 0;
        bool isSecond = false;
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = cardNumber[i] - '0';
            if (isSecond == true)
            {
                digit *= 2;
            }

            sum += digit / 10;
            sum += digit % 10;
            isSecond = !isSecond;
        }

        if (sum % 10 != 0)
        {
            return false;
        }


        return true;
    }

    private static bool IsValidExpirationDate(string expirationDate)
    {
        if (string.IsNullOrWhiteSpace(expirationDate))
        {
            return false;
        }

        var parts = expirationDate.Split('/');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
        {
            return false;
        }

        if (month < 1 || month > 12)
        {
            return false;
        }

        return true;
    }
}