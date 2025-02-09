using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Linq.Expressions;
using TransactionStore.Persistence.Data;
using TransactionStore.Persistence.Interfaces;

namespace TransactionStore.Persistence.Repositories;
public class BaseRepository<TEntity>(
        ApplicationDbContext Context,
        ILogger<BaseRepository<TEntity>> logger,
        IConfiguration configuration
    ) : IBaseRepository<TEntity> where
        TEntity : class
{
    protected readonly DbSet<TEntity> DbSet = Context.Set<TEntity>();
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection");
    public virtual async Task<TEntity?> GetByIdAsync(object id)
    {
        logger.LogDebug("Entering GetByIdAsync with id: {Id}", id);
        var entity = await DbSet.FindAsync(id);
        logger.LogDebug("Exiting GetByIdAsync with entity: {Entity}", entity);
        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        logger.LogDebug("Entering GetAllAsync");
        var entities = await DbSet.ToListAsync();
        logger.LogDebug("Exiting GetAllAsync with entities: {Entities}", entities);
        return await DbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        logger.LogDebug("Entering FindAsync with predicate: {Predicate}", predicate);
        var entities = await DbSet.Where(predicate).ToListAsync();
        logger.LogDebug("Exiting FindAsync with entities: {Entities}", entities);
        return entities;
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        logger.LogDebug("Entering AddAsync with entity: {Entity}", entity);
        await DbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
        logger.LogDebug("Entity added successfully: {Entity}", entity);
    }

    public virtual async Task<TEntity> AddAndReturnAsync(TEntity entity)
    {
        logger.LogDebug("Entering AddAndReturnAsync with entity: {Entity}", entity);
        await DbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
        logger.LogDebug("Entity added and returned successfully: {Entity}", entity);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        logger.LogDebug("Entering AddRangeAsync with entities: {Entities}", entities);
        await DbSet.AddRangeAsync(entities);
        await Context.SaveChangesAsync();
        logger.LogDebug("Entities added successfully: {Entities}", entities);
    }

    public virtual async Task<T> ExecuteStoredProcedureWithOutputAsync<T>(string storedProcedure, DynamicParameters parameters)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync("CALL public.sp_createtransaction(@accountid, @amount, @transactiontype, @lasttransactionid, @outputid)", parameters);
            return parameters.Get<T>("outputid");
        }
    }

    public async Task TransactionalAddRangeAsync(IEnumerable<TEntity> entities)
    {
        logger.LogDebug("Entering TransactionalAddRangeAsync with entities: {Entities}", entities);
        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            await DbSet.AddRangeAsync(entities);
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
            logger.LogInformation("Entities added transactionally successfully: {Entities}", entities);
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error occurred while adding entities transactionally: {Entities}", entities);
            throw;
        }
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        logger.LogDebug("Entering UpdateAsync with entity: {Entity}", entity);
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
        logger.LogDebug("Entity updated successfully: {Entity}", entity);
    }

    public virtual async Task RemoveAsync(TEntity entity)
    {
        logger.LogDebug("Entering RemoveAsync with entity: {Entity}", entity);
        DbSet.Remove(entity);
        await Context.SaveChangesAsync();
        logger.LogDebug("Entity removed successfully: {Entity}", entity);
    }
}
