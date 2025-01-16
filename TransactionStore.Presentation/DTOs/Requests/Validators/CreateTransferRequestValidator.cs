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
        RuleFor(x => x.CurrencyPair)
            .NotEmpty().WithMessage("Валютная пара обязательна для заполнения.")
            .Length(6).WithMessage("Валюта пара должна состоять из 6 символов (например RUBEUR).")
            .Must(BeAValidCurrencyPair).WithMessage("Валютной пара не существует.");
    }

    private bool BeAValidCurrencyPair(string currencyPair)
    {
        if (currencyPair.Length != 6) return false;

        var baseCurrency = currencyPair.Substring(0, 3);
        var nextCurrency = currencyPair.Substring(3, 3);

        return Enum.TryParse<Currencies>(baseCurrency, out _) && Enum.TryParse<Currencies>(nextCurrency, out _);
    }
}
