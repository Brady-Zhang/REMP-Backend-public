using FluentValidation;
using RealEstate.DTOs.User;

namespace RealEstate.Validators
{
    public class UserRegisterValidator : AbstractValidator<UserRegisterRequestDto>
    {
        public UserRegisterValidator() {
           
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.");
                //.Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number format.");
        }
    }
}
