namespace Shared;

public static class RabbitMqSettings
{
    public const string Stock_ProductCreatedEventQueue = "stock-product-created-event-queue";
    public const string Product_StockCreationFailedEvent = "product-stock-creation-failed-event";
    public const string Stock_ProductUpdatedEventQueue = "stock-product-updated-event-queue";
    public const string Stock_ProductDeletedEventQueue = "stock-product-deleted-event-queue";
    public const string Stock_OrderStartedEventQueue = "order-started-event-queue";

}