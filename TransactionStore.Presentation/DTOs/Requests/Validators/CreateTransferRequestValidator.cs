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
            .NotEmpty().WithMessage("Валютная пара обязательна для заполнения.")
            .Length(6).WithMessage("Валюта пара должна состоять из 3 символов (например RUB).")
            .Must(BeAValidCurrency).WithMessage("Валюта указана неверно или не поддерживается.");
        RuleFor(x => x.ToCurrency)
            .NotEmpty().WithMessage("Валютная пара обязательна для заполнения.")
            .Length(6).WithMessage("Валюта пара должна состоять из 3 символов (например RUB).")
            .Must(BeAValidCurrency).WithMessage("Валюта указана неверно или не поддерживается.");
    }

    private bool BeAValidCurrency(string currency)
    {
        if (currency.Length != 3) return false;

        return Enum.TryParse<Currencies>(currency, out _);
    }
}
