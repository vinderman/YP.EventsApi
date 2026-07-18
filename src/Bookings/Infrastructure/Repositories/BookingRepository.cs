using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Repositories;

public class BookingRepository: IBookingRepository
{
    private readonly AppDbContext _context;
    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IReadOnlyList<Booking>> GetAllByStatusAsync(BookingStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bookings.Where(b => b.Status == status).ToListAsync(cancellationToken);
    }


    public async Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
    }

    public async Task CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await _context.Bookings.AddAsync(booking, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Where(b => b.UserId == userId)
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)
            .CountAsync(cancellationToken);
    }
}
