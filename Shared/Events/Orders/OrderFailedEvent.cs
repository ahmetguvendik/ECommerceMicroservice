namespace Shared.Events;

public class OrderFailedEvent
{
    public Guid OrderId { get; set; }
    public string Message { get; set; }
}