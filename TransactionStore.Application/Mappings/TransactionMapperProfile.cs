using TransactionStore.Application.Models;
using TransactionStore.Persistence.Entities;
using AutoMapper;
using MYPBackendMicroserviceIntegrations.Messages;

namespace TransactionStore.Application.Mappings;
public class TransactionMapperProfile: Profile
{
    public TransactionMapperProfile()
    {
        CreateMap<Transaction, TransactionModel>().ReverseMap();
        CreateMap<Transaction, TransactionCreatedMessage>();
    }
}
