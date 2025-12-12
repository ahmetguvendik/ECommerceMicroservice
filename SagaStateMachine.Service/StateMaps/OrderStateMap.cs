using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SagaStateMachine.Service.StateInstances;

namespace SagaStateMachine.Service.StateMaps;

public class OrderStateMap : SagaClassMap<OrderStateInstance>
{
    protected override void Configure(EntityTypeBuilder<OrderStateInstance> entity, ModelBuilder model)
    {
        entity.Property(x => x.CustomerId).IsRequired().HasMaxLength(50);
        entity.Property(x => x.TotalAmount).IsRequired();
        entity.Property(x => x.OrderId).IsRequired();
        base.Configure(entity, model);
    }
}