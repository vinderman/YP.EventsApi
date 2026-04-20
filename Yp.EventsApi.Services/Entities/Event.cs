using Yp.EventsApi.Services.Exceptions;

namespace Yp.EventsApi.Services.Entities;

public class Event
{
    private Event()
    {
        
    }
    public static Event CreateInstance(Guid id , string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null)
    {
        if (totalSeats <= 0)
        {
            throw new DomainValidationException("Количество мест на событии не может быть отрицательным");
        }
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            StartAt = startAt,
            EndAt = endAt,
            Description = description,
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats
        };
    }
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
    
    public required int TotalSeats { get; set; }
    
    public int AvailableSeats { get; set; }

    public bool TryReserveSeats(int count = 1)
    {
        if (count <= 0)
        {
            return false;
        }
        
        if (AvailableSeats < count)
        {
            return false;
        }
        
        AvailableSeats-= count;

        return true;
    }

    public void ReleaseSeats(int count = 1)
    {
        AvailableSeats += count;
    }
}