using AutoMapper;
using TransactionStore.Application.Interfaces;
using TransactionStore.Application.Models;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Application.Services;
public class TransactionService: ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    public TransactionService(ITransactionRepository transactionRepository,IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }
    public async Task<TransactionModel?> GetTransactionByIdAsync(Guid id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        return _mapper.Map<TransactionModel>(transaction);
    }
    public async Task<decimal> CountUserBalance(Guid accountId)
    {
        var transactions = await _transactionRepository.FindAsync(t => t.AccountId == accountId);
        return transactions.Sum(t => t.Amount);
    }
    public async Task<IEnumerable<TransactionModel>> GetTransactionsByAccountIdAsync(Guid accountId)
    {
        var transactions = await _transactionRepository.FindAsync(t => t.AccountId == accountId);
        return _mapper.Map<IEnumerable<TransactionModel>>(transactions);
    }

    public async Task<TransactionModel> CreateTransaction(TransactionModel transactionModel)
    {
        var transaction = _mapper.Map<Transaction>(transactionModel);
        await _transactionRepository.AddAsync(transaction);
        return transactionModel;
    }

    public async Task<IEnumerable<TransactionModel>> CreateTransferTransaction(IEnumerable<TransactionModel> transactionModels)
    {
        var transactions = _mapper.Map<IEnumerable<Transaction>>(transactionModels);
        await _transactionRepository.AddRangeAsync(transactions);
        return transactionModels;
    }

}
