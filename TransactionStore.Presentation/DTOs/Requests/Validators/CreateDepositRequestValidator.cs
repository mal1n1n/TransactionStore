using FluentValidation;

namespace TransactionStore.Presentation.DTOs.Requests.Validators;
public class CreateDepositRequestValidator : AbstractValidator<CreateDepositRequest>
{
    public CreateDepositRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("AccountId обязателен для заполнения.");
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше нуля.");
    }
}