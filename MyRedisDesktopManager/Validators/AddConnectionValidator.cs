using FluentValidation;
using MyRedisDesktopManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.Validators
{
	public class AddConnectionValidator : AbstractValidator<EditConnectionViewModel>
	{
		public AddConnectionValidator()
		{
			this.RuleFor(t => t.Name).NotNull().NotEmpty().MaximumLength(16);
			this.RuleFor(t => t.Host).NotNull().NotEmpty().MaximumLength(32);
			this.RuleFor(t => t.Port).GreaterThan(0).LessThan(1000000);
			this.RuleFor(t => t.Password).MaximumLength(64);

			this.RuleFor(t => t.Timeout).GreaterThan(0).LessThan(600);
			this.RuleFor(t => t.KeySeparator).MaximumLength(4);
		}
	}
}
