namespace DeveloperStore.Api.Services;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public bool Enabled { get; init; }
    public string Exchange { get; init; } = "developer_store.events";
}

