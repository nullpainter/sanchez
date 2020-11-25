using FluentValidation;
using Sanchez.Models.CommandLine;

namespace Sanchez.Validators
{
    public class EquirectangularOptionsValidator : CommandLineOptionsValidator<EquirectangularOptions>
    {
        public EquirectangularOptionsValidator()
        {
            RuleFor(o => o.AutoCrop)
                .Must(crop => !crop)
                .When(o => o.NoCrop)
                .WithMessage(o => "Automatic cropping cannot be performed if no cropping is requested");
        }
}
}