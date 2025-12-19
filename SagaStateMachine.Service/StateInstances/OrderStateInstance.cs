using MassTransit;
using Shared.Messages;

namespace SagaStateMachine.Service.StateInstances;

public class OrderStateInstance : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrentState { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<OrderItemMessage> OrderItemMessages { get; set; } = new();
}   