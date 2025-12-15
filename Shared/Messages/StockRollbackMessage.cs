namespace Shared.Messages;

public class StockRollbackMessage
{
    public List<OrderItemMessage> OrderItemMessages { get; set; }   
}