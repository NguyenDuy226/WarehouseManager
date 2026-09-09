using System.Globalization;
using System.Text.RegularExpressions;
using Warehouse.Application.Interfaces;


namespace Warehouse.Infrastructure.Services
{
    public class ValidatorName : IValidatorName
    {
        public string ValidHumanName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) 
                return string.Empty;
            string cleanString = Regex.Replace(input.Trim(), @"\s+", " ");
            string lowerCaseString = cleanString.ToLower();
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(lowerCaseString);
        }

        public string ValidName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) 
                return string.Empty;
            string cleanString = Regex.Replace(input.Trim(), @"\s+", " ");
            string lowerCaseString = cleanString.ToLower();
            return char.ToUpper(lowerCaseString[0]) + lowerCaseString.Substring(1);
        }
    }
}