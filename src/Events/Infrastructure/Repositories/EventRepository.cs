using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _dbContext;

    public EventRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<Event> Items, int Total)> GetPagedAsync(
        EventFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(_dbContext.Events.AsQueryable(), filter);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    private static IQueryable<Event> ApplyFilter(IQueryable<Event> query, EventFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            query = query.Where(e => EF.Functions.ILike(e.Title, $"%{filter.Title}%"));
        }

        if (filter.From.HasValue)
            query = query.Where(e => e.StartAt >= filter.From);

        if (filter.To.HasValue)
            query = query.Where(e => e.EndAt <= filter.To);

        return query;
    }

    public async Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
    }

    public async Task<Event?> GetByIdForUpdateAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .FromSqlInterpolated($"SELECT * FROM \"Events\" WHERE \"Id\" = {eventId} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Event addEvent, CancellationToken cancellationToken = default)
    {
        await _dbContext.Events.AddAsync(addEvent, cancellationToken);
    }

    public void Remove(Event eventToDelete)
    {
        _dbContext.Remove(eventToDelete);
    }

    public async Task<IReadOnlyList<Event>> GetTopSelledEvents(int count, CancellationToken cancellationToken)
    {
       return await _dbContext.Events.OrderByDescending(e => (e.TotalSeats - e.AvailableSeats) / e.TotalSeats).Take(count).ToListAsync(cancellationToken);
    }
}
