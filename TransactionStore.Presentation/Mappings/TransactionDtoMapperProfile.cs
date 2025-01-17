using AutoMapper;
using TransactionStore.Application.Models;
using TransactionStore.Presentation.DTOs.Requests;
using TransactionStore.Presentation.DTOs.Responses;

namespace TransactionStore.Presentation.Mappings;
public class TransactionDtoMapperProfile: Profile
{
    public TransactionDtoMapperProfile()
    {
        CreateMap<TransactionModel, TransactionResponse>();
        CreateMap<TransactionModel, ShortTransactionResponse>();
    }
}
