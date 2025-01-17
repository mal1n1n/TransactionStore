using FluentValidation;
using TransactionStore.Core;

namespace TransactionStore.Presentation.DTOs.Requests.Validators;
public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty().WithMessage("FromAccountId обязателен для заполнения.");
        RuleFor(x => x.ToAccountId)
            .NotEmpty().WithMessage("ToAccountId обязателен для заполнения.");
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше нуля.");
        RuleFor(x => x.FromCurrency)
            .NotEqual(Currencies.Unknown).WithMessage("Валюта указана неверно или не поддерживается.")
            .NotEqual(x => x.ToCurrency).WithMessage("Валюты должны быть разными.")
            .IsInEnum().WithMessage("Валюта указана неверно или не поддерживается.");
        RuleFor(x => x.ToCurrency)
            .NotEqual(Currencies.Unknown).WithMessage("Валюта указана неверно или не поддерживается.")
            .NotEqual(x => x.FromCurrency).WithMessage("Валюты должны быть разными.")
            .IsInEnum().WithMessage("Валюта указана неверно или не поддерживается.");
    }
}
