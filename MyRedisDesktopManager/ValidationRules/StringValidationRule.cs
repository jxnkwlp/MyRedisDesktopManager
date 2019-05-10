using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyRedisDesktopManager.ValidationRules
{
	public class StringValidationRule : ValidationRule
	{
		public bool Required { get; set; }
		public string RequiredMessage { get; set; }

		public int? MinLength { get; set; }
		public string MinLengthMessage { get; set; }

		public int? MaxLength { get; set; }
		public string MaxLengthMessage { get; set; }

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if (value == null && Required)
			{
				return new ValidationResult(false, RequiredMessage ?? "Required!");
			}

			if (value != null)
			{
				if ((MinLength.HasValue && MinLength.Value > 0 && value.ToString().Length < MinLength.Value))
				{
					return new ValidationResult(false, MinLengthMessage ?? $"The input min length is {MinLength}");
				}

				if ((MaxLength.HasValue && MaxLength.Value > 0 && value.ToString().Length > MaxLength.Value))
				{
					return new ValidationResult(false, MaxLengthMessage ?? $"The input max length is {MaxLength}");
				}
			}


			return ValidationResult.ValidResult;
		}
	}
}
