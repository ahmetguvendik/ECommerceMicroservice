# ECommerce Microservice – Saga Akışı Özeti

Bu repo, MassTransit + RabbitMQ ile orchestrated saga kullanan 6 servis (Basket, Order, Stock, Product, Payment, Delivery) ve bir state machine servisinden oluşur. Aşağıdaki özet, uçtan uca sipariş akışını ve kullanılan event/queue/consumer’ları anlatır.

## Genel Akış (Checkout → Teslimat)
1) **BasketService**: Checkout’ta `OrderStartedEvent` publish eder → `order-started-event-queue`.
2) **Saga**: `OrderStartedEvent` alır, `CorrelationId` üretir, `OrderCreatedCommandEvent`’i `order-create-command-queue`’ya gönderir. State: `OrderCreated`.
3) **OrderService**: `CreateOrderCommandConsumer` bu komutu alır, DB’de siparişi oluşturur, `OrderCreatedEvent` publish eder (CorrelationId ile).
4) **Saga**: `OrderCreatedEvent`’i yakalar, stok için aynı event’i `stock-order-created-event-queue`’ya forward eder.
5) **StockService**: `OrderCreatedEventConsumer` stok rezervasyonu yapar. Yeterliyse `StockReservedEvent`, yetersizse `StockNotReservedEvent` publish eder.
6) **Saga**:
   - `StockReservedEvent` → `PaymentStartedEvent`’i `payment-started-event-queue`’ya gönderir, state `StockReserved`.
   - `StockNotReservedEvent` → `OrderFailedEvent` publish eder, state `StockNotReserved`.
7) **PaymentService** (senin implementasyonun): `PaymentStartedEvent`’i dinler, başarılıysa `PaymentCompletedEvent`, başarısızsa `PaymentFailedEvent` publish eder.
8) **Saga**:
   - `PaymentCompletedEvent` → `DeliveryStartedEvent`’i `delivery-started-event-queue`’ya gönderir, state `DeliveryStarted`.
   - `PaymentFailedEvent` → `OrderFailedEvent` publish + `StockRollbackMessage`’ı rollback kuyruğuna yollar, state `PaymentFailed`.
9) **DeliveryService** (senin implementasyonun): `DeliveryStartedEvent`’i dinler, başarılıysa `DeliveryCompletedEvent`, başarısızsa `DeliveryFailedEvent` publish eder.
10) **Saga**:
    - `DeliveryCompletedEvent` → `OrderCompletedEvent` publish, state `DeliveryCompleted`.
    - `DeliveryFailedEvent` → `OrderFailedEvent` publish + `StockRollbackMessage`, state `DeliveryFailed`.

## Önemli Queue/Sabitler (`Shared/RabbitMqSettings.cs`)
- `order-started-event-queue`
- `order-create-command-queue`
- `stock-order-created-event-queue`
- `payment-started-event-queue`
- `delivery-started-event-queue`
- `stock-rollback-message-event-queue`
- `order-order-completed-event-queue`, `order-order-failed-event-queue`

## Saga State Machine
- Dosya: `SagaStateMachine.Service/StateMachines/OrderStateMachine.cs`
- Başlıca stateler: `OrderCreated`, `StockReserved`, `PaymentFailed`, `StockNotReserved`, `DeliveryStarted`, `DeliveryCompleted`, `DeliveryFailed`.
- EF repo: optimistic concurrency, `CreatedDate` UTC.

## Servisler ve Consumer’lar
- **BasketService**: Checkout → `OrderStartedEvent`.
- **OrderService**: `CreateOrderCommandConsumer` (OrderCreatedCommandEvent), handler DB yazıp `OrderCreatedEvent` publish. `OrderEventService` tamam/başarısız eventlerinde durum günceller.
- **StockService**: `OrderCreatedEventConsumer` rezervasyon; `StockRollbackMessageConsumer` rollback. `StockEventService` rollback’te stok kaydı yoksa exception fırlatır.
- **PaymentService**: (eklenecek) `payment-started-event-queue` dinleyip `PaymentCompletedEvent` / `PaymentFailedEvent` publish eder.
- **DeliveryService**: (eklenecek) `delivery-started-event-queue` dinleyip `DeliveryCompletedEvent` / `DeliveryFailedEvent` publish eder.
- **ProductService**: Ürün CRUD eventleri mevcut; sipariş akışının dışında.

## Notlar
- Tüm uzun süreçler korelasyonlu; CorrelationId basket’ten saga’ya, order/stock/payment/delivery adımlarına taşınıyor.
- Rollback: Payment veya Delivery başarısız olursa saga `StockRollbackMessage` gönderir; stok kaydı yoksa hata verilir (görünür olması için).
- Nullability uyarıları event sınıflarında var; istersen `required` veya default init ekleyebilirsin.

## İzleme/Çalıştırma
- Saga: `dotnet run --project SagaStateMachine.Service/SagaStateMachine.Service.csproj`
- Order: `dotnet run --project OrderService/OrderService.WebApi/OrderService.WebApi.csproj`
- Stock: `dotnet run --project StockService/StockService.WebApi/StockService.WebApi.csproj`
- RabbitMQ yönetim panelinden ilgili kuyruklarda mesaj akışını takip edebilirsin.

