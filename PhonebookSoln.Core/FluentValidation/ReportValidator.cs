using FluentValidation;
using PhonebookSoln.Core.Entities;

namespace PhonebookSoln.Core.FluentValidation
{
    public class ReportValidator : AbstractValidator<Report>
    {
        public ReportValidator()
        {
            RuleFor(r => r.Location)
                .NotEmpty().WithMessage("Title is required.")
                .Length(3, 200).WithMessage("Title must be between 3 and 200 characters.");
        }
    }
}
