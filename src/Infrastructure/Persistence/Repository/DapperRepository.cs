using Application.Common.Persistence;
using Dapper;
using Domain.Common.Contracts;
using Infrastructure.Persistence.Context;
using System.Data;

namespace Infrastructure.Persistence.Repository;
public class DapperRepository(ApplicationDbContext dbContext) : IDapperRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity =>
        (await _dbContext.Connection.QueryAsync<T>(sql, param, transaction))
            .AsList();

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity
    {
        return await _dbContext.Connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);
    }

    public Task<T> QuerySingleAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity
    {
        return _dbContext.Connection.QuerySingleAsync<T>(sql, param, transaction);
    }
}