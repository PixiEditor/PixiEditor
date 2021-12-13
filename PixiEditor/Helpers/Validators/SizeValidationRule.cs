using System.Globalization;
using System.Windows.Controls;

namespace PixiEditor.Helpers.Validators
{
    public class SizeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int i = int.Parse(((string)value).Split(' ')[0]);

            return new ValidationResult(i > 0, null); // Size is greater than 0
        }
    }
}