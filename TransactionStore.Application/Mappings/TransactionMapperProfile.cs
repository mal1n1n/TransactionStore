using TransactionStore.Application.Models;
using TransactionStore.Persistence.Entities;
using AutoMapper;
namespace TransactionStore.Application.Mappings;
public class TransactionMapperProfile: Profile
{
    public TransactionMapperProfile()
    {
        CreateMap<Transaction, TransactionModel>();
        CreateMap<TransactionModel, Transaction>();
    }
}
