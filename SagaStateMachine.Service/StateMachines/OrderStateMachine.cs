using System;
using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared;
using Shared.Events;
using Shared.Events.Orders;
using Shared.Events.Payments;
using Shared.Events.Stocks;
using Shared.Messages;
using System.Collections.Generic;

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
    
    //State
    public State OrderCreated { get; set; }
    public State StockReserved { get; set; }
    public State PaymentCompleted { get; set; }
    public State PaymentFailed { get; set; }
    public State StockNotReserved { get; set; }
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
        
        //Events
        Event(() => OrderStartedEvent,
            x => x.CorrelateBy<Guid>(x => x.OrderId, @event => @event.Message.OrderId).SelectId(e => Guid.NewGuid()));
        Event(() => OrderCreatedEvent, x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => StockReservedEvent, x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => StockNotReservedEvent, x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => PaymentCompletedEvent, x => x.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => PaymentFailedEvent  , x => x.CorrelateById(@event => @event.Message.CorrelationId));

        //States
        Initially(When(OrderStartedEvent).Then<OrderStateInstance, OrderStartedEvent>(context =>
        {
            context.Instance.CorrelationId = context.CorrelationId ?? Guid.NewGuid();
            context.Instance.OrderId = context.Message.OrderId;
            context.Instance.CustomerId = context.Message.CustomerId;
            context.Instance.TotalAmount = context.Message.TotalAmount;
            context.Instance.CreatedDate = DateTime.UtcNow;
        }).TransitionTo(OrderCreated)
        .Send(new Uri($"queue:{RabbitMqSettings.Order_CreateOrderCommandQueue}"),
            context => new CreateOrderCommandMessage()
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
                })
                .Send(new Uri($"queue:{RabbitMqSettings.Stock_OrderCreatedEventQueue}"), context => context.Message));


    }
}