using System.Globalization;
using System.Windows.Controls;

namespace PixiEditor.Helpers.Validators
{
    public class SizeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new ValidationResult(int.Parse(((string)value).Split(' ')[0]) > 0, null); // Size is greater than 0
        }
    }
}