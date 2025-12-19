using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared;
using Shared.Events;
using Shared.Events.Orders;
using Shared.Events.Payments;
using Shared.Events.Deliveries;
using Shared.Events.Stocks;
using Shared.Messages;
using System.Collections.Generic;
using System.Linq;

namespace SagaStateMachine.Service.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
{
    //Events
    public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
    public Event<OrderCreatedEvent> OrderCreatedEvent { get; set; }
    public Event<StockReservedEvent>  StockReservedEvent { get; set; }
    public Event<PaymentCompletedEvent>  PaymentCompletedEvent { get; set; }
    public Event<PaymentFailedEvent>  PaymentFailedEvent { get; set; }
    public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
    public Event<DeliveryCompletedEvent> DeliveryCompletedEvent { get; set; }
    public Event<DeliveryFailedEvent> DeliveryFailedEvent { get; set; }
    
    //State
    public State OrderCreated { get; set; }
    public State StockReserved { get; set; }
    public State PaymentCompleted { get; set; }
    public State PaymentFailed { get; set; }
    public State StockNotReserved { get; set; }
    public State DeliveryStarted { get; set; }
    public State DeliveryCompleted { get; set; }
    public State DeliveryFailed { get; set; }
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        SetCompletedWhenFinalized();
        
        //Events
        Event(() => OrderStartedEvent,
            x => x.CorrelateBy<Guid>(x => x.OrderId, @event => @event.Message.OrderId).SelectId(e => Guid.NewGuid()));
        Event(() => OrderCreatedEvent, x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => StockReservedEvent, x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => StockNotReservedEvent, x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => PaymentCompletedEvent, x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => PaymentFailedEvent  , x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => DeliveryCompletedEvent, x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => DeliveryFailedEvent  , x => x.CorrelateById(@event => @event.Message.CorrelationId));

        //States
        Initially(When(OrderStartedEvent).Then<OrderStateInstance, OrderStartedEvent>(context =>
        {
            context.Instance.CorrelationId = context.CorrelationId ?? Guid.NewGuid();
            context.Instance.OrderId = context.Message.OrderId;
            context.Instance.CustomerId = context.Message.CustomerId;
            context.Instance.TotalAmount = context.Message.TotalAmount;
            context.Instance.CreatedDate = DateTime.UtcNow;
            context.Instance.OrderItemMessages = context.Data.Items ?? new List<OrderItemMessage>();
        }).TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMqSettings.Order_CreateOrderCommandQueue}"),
            context => new OrderCreatedCommandEvent
                ()
            {
                CorrelationId = context.Instance.CorrelationId,
                OrderId = context.Instance.OrderId,
                CustomerId = context.Instance.CustomerId,
                TotalAmount = context.Instance.TotalAmount,
                Items = context.Data.Items ?? new List<OrderItemMessage>()
            }));

        During(OrderCreated,
            When(OrderCreatedEvent)
                .Then(context =>
                {
                    context.Instance.OrderId = context.Message.OrderId;
                    context.Instance.TotalAmount = context.Message.TotalPrice;
                    context.Instance.OrderItemMessages = context.Instance.OrderItemMessages.Any()
                        ? context.Instance.OrderItemMessages
                        : context.Data.OrderItemMessages ?? new List<OrderItemMessage>();
                })
                .Send(new Uri($"queue:{RabbitMqSettings.Stock_OrderCreatedEventQueue}"), context => context.Message),

            When(StockReservedEvent)
                .Then(ctx =>
                {
                    ctx.Instance.TotalAmount = ctx.Instance.TotalAmount; // keep total for payment
                    ctx.Instance.OrderItemMessages = ctx.Data.OrderItemMessages ?? ctx.Instance.OrderItemMessages;
                })
                .Send(new Uri($"queue:{RabbitMqSettings.Payment_StartedEvenetQueue}"),
                    ctx => new PaymentStartedEvent(ctx.Instance.CorrelationId)
                    {
                        OrderId = ctx.Instance.OrderId,
                        TotalPrice = ctx.Instance.TotalAmount,
                        OrderItemMessages = ctx.Data.OrderItemMessages
                    })
                .TransitionTo(StockReserved),

            When(StockNotReservedEvent)
                .Publish(ctx => new OrderFailedEvent
                {
                    OrderId = ctx.Instance.OrderId,
                    Message = ctx.Data.Message
                })
                .Finalize());

        During(StockReserved,
            When(PaymentCompletedEvent)
                .Send(new Uri($"queue:{RabbitMqSettings.Delivery_StartedEventQueue}"),
                    ctx => new DeliveryStartedEvent(ctx.Instance.CorrelationId)
                    {
                        OrderId = ctx.Instance.OrderId,
                        OrderItemMessages = ctx.Instance.OrderItemMessages ?? new List<OrderItemMessage>()
                    })
                .TransitionTo(DeliveryStarted),

            When(PaymentFailedEvent)
                .Publish(ctx => new OrderFailedEvent
                {
                    OrderId = ctx.Instance.OrderId,
                    Message = ctx.Data.Message
                })
                .Send(new Uri($"queue:{RabbitMqSettings.Stock_RollbackMessageEventQueue}"),
                    ctx => new StockRollbackMessage
                    {
                        // Gönderilen stok item'larını iade kuyruğuna taşı
                        OrderItemMessages = ctx.Data.OrderItemMessages ?? new List<OrderItemMessage>()
                    })
                .Finalize());

        During(DeliveryStarted,
            When(DeliveryCompletedEvent)
                .Publish(ctx => new OrderCompletedEvent
                {
                    OrderId = ctx.Instance.OrderId
                })
                .Finalize(),

            When(DeliveryFailedEvent)
                .Publish(ctx => new OrderFailedEvent
                {
                    OrderId = ctx.Instance.OrderId,
                    Message = ctx.Data.Message
                })
                .Send(new Uri($"queue:{RabbitMqSettings.Stock_RollbackMessageEventQueue}"),
                    ctx => new StockRollbackMessage
                    {
                        OrderItemMessages = ctx.Data.OrderItemMessages ?? ctx.Instance.OrderItemMessages ?? new List<OrderItemMessage>()
                    })
                .Finalize());
    }
}