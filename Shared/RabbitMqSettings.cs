namespace Shared;

public static class RabbitMqSettings
{
    public const string StateMachineQueue = "state-machine-queue";
    public const string Stock_ProductCreatedEventQueue = "stock-product-created-event-queue";
    public const string Product_StockCreationFailedEvent = "product-stock-creation-failed-event";
    public const string Stock_ProductUpdatedEventQueue = "stock-product-updated-event-queue";
    public const string Stock_ProductDeletedEventQueue = "stock-product-deleted-event-queue";
    public const string Order_OrderStartedEventQueue = "order-started-event-queue";
    public const string Order_CreateOrderCommandQueue = "order-create-command-queue";
    public const string Stock_OrderCreatedEventQueue = "stock-order-created-event-queue";
    public const string Order_OrderCompletedEventQueue = "order-order-completed-event-queue";
    public const string Order_OrderFailedEventQueue = "order-order-failed-event-queue";
    public const string Stock_RollbackMessageEventQueue = "stock-rollback-message-event-queue";
    public const string Payment_StartedEvenetQueue = "payment-started-event-queue";
    
}