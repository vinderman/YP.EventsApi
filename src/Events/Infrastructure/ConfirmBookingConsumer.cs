using System.Text.Json;
using Application.Services.EventService;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Exceptions;
using Shared.Messages;
using Shared.Options;

namespace Infrastructure;

public class ConfirmBookingConsumer: BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<ConfirmBookingConsumer> _logger;
    
    public ConfirmBookingConsumer(IOptions<KafkaSettings> kafkaSettings, ILogger<ConfirmBookingConsumer> logger, IServiceScopeFactory scopeFactory)
    {
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => Consume(stoppingToken), stoppingToken);
    }
    
    private async Task Consume(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            // Все консьюмеры с одинаковым GroupId делят партиции топика между собой
            GroupId = _kafkaSettings.ConsumerGroup,
            // Earliest — при первом запуске читать с начала топика
            // (если у группы ещё нет сохранённого офсета)
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // false — управляем коммитом офсета вручную (at-least-once)
            EnableAutoCommit = false,
            // false — управляем позицией смещения вручную
            EnableAutoOffsetStore = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        // Kafka назначит консьюмеру партиции в рамках перебалансировки группы
        consumer.Subscribe(KafkaTopics.ConfirmBooking);

        _logger.LogInformation($"Consumer запущен. Ожидание сообщений из топика {KafkaTopics.ConfirmBooking}'...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Блокирующий вызов: ждём следующего сообщения от брокера (pull-модель)
                // При отмене CancellationToken бросает OperationCanceledException
                var consumeResult = consumer.Consume(stoppingToken);

                var booking = JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value);

                _logger.LogInformation(
                    "Получен запрос на бронирование [{Offset}] BookingId={BookingId}, EventId={EventId}, UserId={UserId}",
                    consumeResult.TopicPartitionOffset,
                    booking?.BookingId,
                    booking?.EventId,
                    booking?.UserId);
                
                using var scope = _scopeFactory.CreateScope();
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

                try
                {
                    await eventService.TryReserveSeats(booking.EventId, 1, stoppingToken);
                    // Сохраняем офсет в локальный буфер — не обращение к брокеру
                    // Консьюмер отправит накопленные офсеты в Kafka в фоне по таймеру
                    // При сбое до этой строки сообщение будет повторно доставлено (at-least-once)
                    consumer.StoreOffset(consumeResult);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Ошибка JSON, сообщение пропущено: {Value}", consumeResult.Message.Value);
                    consumer.StoreOffset(consumeResult); // повтор бессмысленен
                }
                catch (NoAvailableSeatsException ex)
                {
                    _logger.LogInformation(ex, "Нет мест, сообщение считаем обработанным (BookingId={BookingId})", /*bookingId*/ null);
                    consumer.StoreOffset(consumeResult); // бизнес-отказ, повтор обычно не нужен
                }
                catch (EntityNotFoundException ex)
                {
                    _logger.LogWarning(ex, "Событие не найдено, сообщение считаем обработанным");
                    consumer.StoreOffset(consumeResult);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw; // штатная остановка сервиса
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Техническая ошибка, офсет не коммитим (повторим позже)");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Штатное завершение — CancellationToken был отменён хостом
            _logger.LogInformation("Consumer остановлен штатно.");
        }
        finally
        {
            // Close() отправляет leave group — rebalance происходит немедленно,
            // без ожидания session.timeout.ms. Также коммитит буферизованные офсеты
            consumer.Close();
        }
    }
}