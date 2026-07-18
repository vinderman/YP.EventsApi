using Microsoft.EntityFrameworkCore.Storage;
using Shared.UnitOfWork;

namespace Infrastructure;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public EfUnitOfWork(AppDbContext context)
    {
        _context = context;
    }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new EfUnitOfWorkTransaction(transaction);
    }
}
internal sealed class EfUnitOfWorkTransaction : IUnitOfWorkTransaction
{
    private readonly IDbContextTransaction _transaction;
    private bool _completed;
    public EfUnitOfWorkTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotCompleted();
        _completed = true;
        return _transaction.CommitAsync(cancellationToken);
    }
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotCompleted();
        _completed = true;
        return _transaction.RollbackAsync(cancellationToken);
    }
    public async ValueTask DisposeAsync()
    {
        // Если забыли Commit/Rollback — откатываем при dispose
        if (!_completed)
        {
            await _transaction.RollbackAsync();
        }
        await _transaction.DisposeAsync();
    }
    private void EnsureNotCompleted()
    {
        if (_completed)
            throw new InvalidOperationException("Транзакция уже завершена.");
    }
}
