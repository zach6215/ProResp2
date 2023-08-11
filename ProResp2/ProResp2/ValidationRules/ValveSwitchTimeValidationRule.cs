using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProResp2
{
    internal class ValveSwitchTimeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value.ToString(), out int valveSwitchTimeMin))
            {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "Not an integer.");
        }
    }
}