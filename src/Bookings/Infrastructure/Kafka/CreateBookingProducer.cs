using System.Text.Json;
using Application.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Shared.Messages;
using Shared.Options;

namespace Infrastructure.Kafka;

public class CreateBookingProducer: ICreateBookingProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    public CreateBookingProducer(IOptions<KafkaSettings> configuration)
    {
        var kafkaSettings = configuration.Value;
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            Acks = Acks.All
        };
        
        _producer = new ProducerBuilder<string, string>(config).Build();
    }
    
    public async Task Produce(string topic, BookingConfirmed message)
    {
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = message.EventId.ToString(),
            Value = JsonSerializer.Serialize(message)
        }); 
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}