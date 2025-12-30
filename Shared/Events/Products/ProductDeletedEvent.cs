namespace Shared.Events;

public class ProductDeletedEvent
{
    public Guid Id { get; set; }
    public Guid IdempotentToken { get; set; }
}