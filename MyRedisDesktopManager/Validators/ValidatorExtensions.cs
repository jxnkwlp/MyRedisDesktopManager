using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace MyRedisDesktopManager.Validators
{
	public static class ValidatorExtensions
	{
		public static ValidationResult ValidateFields<T>(this AbstractValidator<T> validation, T instance, params string[] fields)
		{
			var context = new ValidationContext<T>(instance, new PropertyChain(), new MemberNameValidatorSelector(fields));

			return validation.Validate(context);
		}

	}
}
