using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Harjoitus_4_1_1.Binders
{
    public class FloatModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueProvider = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProvider == null)
            {
                return base.BindModel(controllerContext, bindingContext);
            }

            var raw = valueProvider.AttemptedValue;

            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var value = raw;

            // Remove percentage sign
            value = value.Replace("%", "");

            // Remove all whitespace (including NBSP) used as grouping separators
            value = Regex.Replace(value, @"\s+", "").Trim();

            // Keep only digits and separators, but remove sign (optional but helps guard weird inputs)
            value = Regex.Replace(value, @"[^\d\.,]", "");

            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                    $"The value '{raw}' is not a valid decimal number.");
                return null;
            }

            // Normalize decimal separator:
            // - If both ',' and '.' exist, treat the LAST one as decimal separator,
            //   and remove the other as grouping.
            // - If only one exists, treat it as decimal separator.
            var lastComma = value.LastIndexOf(',');
            var lastDot = value.LastIndexOf('.');

            if (lastComma >= 0 && lastDot >= 0)
            {
                if (lastComma > lastDot)
                {
                    // Comma is decimal, dots are grouping: "1.234,56" => "1234.56"
                    value = value.Replace(".", "");
                    value = ReplaceLast(value, ",", ".");
                }
                else
                {
                    // Dot is decimal, commas are grouping: "1,234.56" => "1234.56"
                    value = value.Replace(",", "");
                    // dot already decimal
                }
            }
            else if (lastComma >= 0)
            {
                // Only comma present => decimal comma
                value = ReplaceLast(value, ",", ".");
            }
            // else only dot or no separator => already OK for invariant

            if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            {
                if (result < 0 || result > 100)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                        $"The value '{raw}' is out of range for a percentage (0-100).");
                    return null;
                }
                // Percentage values are divided by 100
                return result / 100;
            }

            bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                $"The value '{raw}' is not a valid decimal number.");

            return null;
        }

        private static string ReplaceLast(string input, string search, string replace)
        {
            var pos = input.LastIndexOf(search, StringComparison.Ordinal);
            if (pos < 0) return input;
            return input.Substring(0, pos) + replace + input.Substring(pos + search.Length);
        }
    }
}