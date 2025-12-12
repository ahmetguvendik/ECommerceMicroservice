using MassTransit;
using SagaStateMachine.Service.StateInstances;

namespace SagaStateMachine.Service.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
    }
}