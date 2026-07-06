namespace Shared.Options;

public class KafkaSettings
{
    public string BootstrapServers { get; set; }
    public string ConsumerGroup { get; set; }
}