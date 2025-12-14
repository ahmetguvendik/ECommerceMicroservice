using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SagaStateMachine.Service.StateInstances;

namespace SagaStateMachine.Service.StateMaps;

public class OrderStateMap : SagaClassMap<OrderStateInstance>
{
    protected override void Configure(EntityTypeBuilder<OrderStateInstance> entity, ModelBuilder model)
    {
        entity.Property(x => x.CustomerId).IsRequired();   
        entity.Property(x => x.OrderId).IsRequired();
        entity.Property(x => x.TotalAmount).HasDefaultValue(0);
        entity.Property(x => x.CurrentState).IsRequired();
        base.Configure(entity, model);
    }
}