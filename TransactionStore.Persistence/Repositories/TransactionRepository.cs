using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using TransactionStore.Persistence.Data;
using TransactionStore.Persistence.Entities;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Persistence.Repositories;
public class TransactionRepository(
        ApplicationDbContext dbContext,
        ILogger<BaseRepository<Transaction>> logger,
        IConfiguration configuration
    ) : BaseRepository<Transaction>(dbContext, logger, configuration), ITransactionRepository
{
    private readonly IDbConnection _dbConnection;

    public TransactionRepository(ApplicationDbContext dbContext, ILogger<BaseRepository<Transaction>> logger, IConfiguration configuration, IDbConnection dbConnection)
        : this(dbContext, logger, configuration)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Guid> CreateTransactionAsync(Transaction transaction, Guid? lastTransactionId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("accountid", transaction.AccountId, DbType.Guid);
        parameters.Add("amount", transaction.Amount, DbType.Decimal);
        parameters.Add("transactiontype", (int)transaction.TransactionType, DbType.Int32);
        parameters.Add("lasttransactionid", lastTransactionId, DbType.Guid);
        parameters.Add("outputid", dbType: DbType.Guid, direction: ParameterDirection.Output);

        await _dbConnection.ExecuteAsync("public.sp_createtransaction", parameters, commandType: CommandType.StoredProcedure);
        return parameters.Get<Guid>("outputid");
    }
    public async Task<(Guid fromTransactionId, Guid toTransactionId)> CreateTransferTransactionAsync(Transaction transFrom, Transaction transTo, Guid? lastTransactionId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("fromaccountid", transFrom.AccountId, DbType.Guid);
        parameters.Add("amountfrom", transFrom.Amount, DbType.Decimal);
        parameters.Add("transactiontypefrom", (int)transFrom.TransactionType, DbType.Int32);
        parameters.Add("toaccountid", transTo.AccountId, DbType.Guid);
        parameters.Add("amountto", transTo.Amount, DbType.Decimal);
        parameters.Add("transactiontypeto", (int)transTo.TransactionType, DbType.Int32);
        parameters.Add("lasttransactionid", lastTransactionId, DbType.Guid);
        parameters.Add("outputfromid", dbType: DbType.Guid, direction: ParameterDirection.Output);
        parameters.Add("outputtoid", dbType: DbType.Guid, direction: ParameterDirection.Output);

        await _dbConnection.ExecuteAsync("public.sp_createtransfertransaction", parameters, commandType: CommandType.StoredProcedure);
        var createdFromId = parameters.Get<Guid>("outputfromid");
        var createdToId = parameters.Get<Guid>("outputtoid");

        return (createdFromId, createdToId);
    }
}
