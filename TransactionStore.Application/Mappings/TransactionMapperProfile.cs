using TransactionStore.Application.Models;
using TransactionStore.Persistence.Entities;
using AutoMapper;
using TransactionStore.Application.Messages;
namespace TransactionStore.Application.Mappings;
public class TransactionMapperProfile: Profile
{
    public TransactionMapperProfile()
    {
        CreateMap<Transaction, TransactionModel>().ReverseMap();
        CreateMap<Transaction, TransactionCreated>();
    }
}
