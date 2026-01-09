# NLog ve Correlation ID Implementasyonu

Bu dokümantasyon, E-Commerce Microservice projesine eklenen **NLog loglama** ve **Correlation ID ile izlenebilirlik** özelliklerinin detaylı açıklamasını içermektedir.

## 📋 İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [Yapılan Değişiklikler](#yapılan-değişiklikler)
3. [Mimari ve Yapı](#mimari-ve-yapı)
4. [Kullanım Kılavuzu](#kullanım-kılavuzu)
5. [Log Formatları](#log-formatları)
6. [Correlation ID Akışı](#correlation-id-akışı)
7. [Örnek Senaryolar](#örnek-senaryolar)
8. [Troubleshooting](#troubleshooting)

---

## 🎯 Genel Bakış

### Ne Eklendi?

✅ **NLog Entegrasyonu**: Tüm mikroservislere yapılandırılmış loglama  
✅ **Correlation ID Desteği**: HTTP ve MassTransit mesajları için otomatik izlenebilirlik  
✅ **Yapılandırılmış Loglama**: JSON ve text formatında log çıktıları  
✅ **Merkezi Middleware**: Shared projesi üzerinden yeniden kullanılabilir bileşenler  
✅ **MassTransit Entegrasyonu**: Consumer'larda otomatik Correlation ID yayılımı  

### Faydaları

🔍 **Dağıtık İzleme**: Bir isteği tüm mikroservisler boyunca takip edebilirsiniz  
🐛 **Hata Ayıklama**: Hangi serviste ne olduğunu kolayca görebilirsiniz  
📊 **Analiz**: Log'ları toplayarak performans ve hata analizi yapabilirsiniz  
🎯 **Production Hazır**: Seq, ElasticSearch, Grafana gibi araçlarla entegre edilebilir  

---

## 🔧 Yapılan Değişiklikler

### 1. Shared Projesi

#### Yeni Dosyalar:
```
Shared/
├── Middlewares/
│   └── CorrelationIdMiddleware.cs          # HTTP request'lerde Correlation ID yönetimi
├── Extensions/
│   ├── CorrelationIdExtensions.cs          # Yardımcı extension metodları
│   └── ApplicationBuilderExtensions.cs     # Middleware extension'ları
└── Filters/
    └── MassTransitCorrelationFilter.cs     # MassTransit consumer'lar için global filter
```

#### Eklenen NuGet Paketleri:
- `NLog` (v5.3.4)
- `NLog.Web.AspNetCore` (v5.3.14)
- `Microsoft.AspNetCore.Http.Abstractions` (v2.2.0)

### 2. Tüm Mikroservisler

#### Her Servise Eklenen Dosyalar:
- `nlog.config` - NLog yapılandırma dosyası (servis bazlı özelleştirilmiş)

#### Program.cs Değişiklikleri:
```csharp
// NLog başlatma
var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
logger.Info("ServiceName starting up...");

try {
    var builder = WebApplication.CreateBuilder(args);
    
    // NLog DI entegrasyonu
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    
    // ... servis konfigürasyonları ...
    
    var app = builder.Build();
    
    // Correlation ID middleware (pipeline'ın başında olmalı)
    app.UseCorrelationId();
    
    // ... diğer middleware'ler ...
    
    logger.Info("ServiceName started successfully");
    app.Run();
}
catch (Exception ex) {
    logger.Error(ex, "ServiceName stopped because of exception");
    throw;
}
finally {
    LogManager.Shutdown();
}
```

#### ServiceRegistration.cs Değişiklikleri (Infrastructure katmanı):
```csharp
using Shared.Filters;

// MassTransit konfigürasyonunda
cfg.UsingRabbitMq((context, hostConfig) =>
{
    hostConfig.Host(configuration.GetConnectionString("RabbitMq"));
    
    // Global Correlation ID filter for all consumers
    hostConfig.UseConsumeFilter(typeof(MassTransitCorrelationFilter<>), context);
    
    // ... endpoint konfigürasyonları ...
});
```

### 3. Güncellenen Servisler

| Servis | Program.cs | ServiceRegistration.cs | nlog.config | Shared Referansı |
|--------|-----------|----------------------|-------------|-----------------|
| OrderService | ✅ | ✅ | ✅ | ✅ |
| ProductService | ✅ | ❌ (Consumer yok) | ✅ | ✅ |
| StockService | ✅ | ✅ | ✅ | ✅ |
| BasketService | ✅ | ✅ | ✅ | ✅ |
| PaymentService | ✅ | ✅ | ✅ | ✅ |
| DeliveryService | ✅ | ✅ | ✅ | ✅ |
| SagaStateMachine.Service | ✅ | ❌ (Farklı yapı) | ✅ | ✅ |
| ProductOutboxPublisher.Service | ✅ | ❌ (Worker Service) | ✅ | ✅ |
| Monitoring.Service | ✅ | ❌ (UI Service) | ✅ | ✅ |

---

## 🏗️ Mimari ve Yapı

### Correlation ID Akış Diyagramı

```
┌─────────────┐
│   Client    │ (X-Correlation-ID header gönderebilir)
└──────┬──────┘
       │ HTTP Request
       ▼
┌─────────────────────────────────────────────┐
│  CorrelationIdMiddleware                    │
│  - Header'dan okur veya yeni GUID üretir   │
│  - HttpContext.Items'a ekler               │
│  - NLog MDLC'ye ekler                      │
│  - Response header'a ekler                 │
└──────┬──────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────┐
│  Controller / Handler                       │
│  - Tüm log'lar otomatik CorrelationId içerir│
└──────┬──────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────┐
│  MassTransit Publisher                      │
│  - CorrelationId mesajla birlikte gönderilir│
└──────┬──────────────────────────────────────┘
       │ RabbitMQ Message
       ▼
┌─────────────────────────────────────────────┐
│  MassTransitCorrelationFilter               │
│  - Mesajdaki CorrelationId'yi alır         │
│  - NLog MDLC'ye ekler                      │
│  - Consumer'a geçer                        │
└──────┬──────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────┐
│  Consumer                                   │
│  - Tüm log'lar otomatik CorrelationId içerir│
└──────┬──────────────────────────────────────┘
       │
       ▼
   (İşlem sonlanır, MDLC temizlenir)
```

### NLog Yapılandırması

Her `nlog.config` dosyası şu target'lara sahiptir:

1. **Console Target**: Geliştirme için renkli konsol çıktısı
2. **All File Target**: Tüm log seviyelerini içeren dosya
3. **Own File Target**: Sadece kendi servis log'ları
4. **JSON File Target**: Yapılandırılmış JSON formatında loglar

---

## 📖 Kullanım Kılavuzu

### 1. HTTP Request'lerde Correlation ID Kullanımı

#### Client tarafında header göndermek:

```bash
curl -X GET "http://localhost:5259/api/order/123" \
  -H "X-Correlation-ID: my-custom-correlation-id-12345"
```

#### Middleware otomatik olarak:
- Eğer header varsa, onu kullanır
- Eğer yoksa, yeni bir GUID üretir
- Response'a ekler
- Tüm log'lara dahil eder

### 2. Controller'da Loglama

```csharp
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;

    public OrderController(ILogger<OrderController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        // CorrelationId otomatik olarak log'a eklenir
        _logger.LogInformation("Getting order with ID: {OrderId}", id);
        
        // İsterseniz manuel olarak da alabilirsiniz
        var correlationId = HttpContext.GetCorrelationId();
        
        return Ok(order);
    }
}
```

### 3. Consumer'da Loglama

Consumer'larda otomatik olarak `MassTransitCorrelationFilter` çalışır:

```csharp
public class CreateOrderCommandConsumer : IConsumer<OrderCreatedCommandEvent>
{
    private readonly ILogger<CreateOrderCommandConsumer> _logger;

    public CreateOrderCommandConsumer(ILogger<CreateOrderCommandConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedCommandEvent> context)
    {
        // CorrelationId otomatik olarak NLog MDLC'ye eklenmiştir
        _logger.LogInformation("Processing order creation - OrderId: {OrderId}", 
            context.Message.OrderId);
        
        // İsterseniz manuel olarak da alabilirsiniz
        var correlationId = context.CorrelationId?.ToString();
        
        // İş mantığı...
    }
}
```

### 4. Background Job'larda Correlation ID

```csharp
public class MyBackgroundJob
{
    private readonly ILogger<MyBackgroundJob> _logger;

    public async Task ExecuteAsync()
    {
        // Yeni bir Correlation ID oluştur
        var correlationId = Guid.NewGuid().ToString();
        CorrelationIdExtensions.SetCorrelationIdToNLog(correlationId);
        
        try
        {
            _logger.LogInformation("Job started");
            // İş mantığı...
        }
        finally
        {
            // Temizle
            CorrelationIdExtensions.ClearCorrelationIdFromNLog();
        }
    }
}
```

---

## 📊 Log Formatları

### Console Log Formatı:
```
2026-01-09 15:30:45.1234|INFO|OrderService|abc-123-def-456|OrderService.Controllers.OrderController|Order created successfully
```

Format: `{timestamp}|{level}|{serviceName}|{correlationId}|{logger}|{message}`

### JSON Log Formatı:
```json
{
  "time": "2026-01-09 15:30:45.1234",
  "level": "INFO",
  "service": "OrderService",
  "correlationId": "abc-123-def-456",
  "logger": "OrderService.Controllers.OrderController",
  "message": "Order created successfully",
  "exception": null,
  "url": "http://localhost:5259/api/order",
  "action": "CreateOrder"
}
```

### Log Dosyaları:

Her servis için `logs/` klasöründe:
```
logs/
├── OrderService-all-2026-01-09.log          # Tüm loglar
├── OrderService-own-2026-01-09.log          # Sadece OrderService logları
├── OrderService-2026-01-09.json             # JSON formatında loglar
└── internal-nlog-OrderService.txt           # NLog internal logları
```

---

## 🔄 Correlation ID Akışı

### Örnek: Basket Checkout'tan Delivery'ye Kadar

```
1. Client → BasketService
   POST /api/basket/checkout
   X-Correlation-ID: (yok, sistem üretir: "abc-123")
   
2. BasketService (Log)
   INFO|BasketService|abc-123|Checkout started for user: user-001

3. BasketService → Saga (RabbitMQ)
   OrderStartedEvent
   CorrelationId: abc-123
   
4. Saga (Log)
   INFO|SagaStateMachine|abc-123|Order started event received

5. Saga → OrderService (RabbitMQ)
   OrderCreatedCommandEvent
   CorrelationId: abc-123
   
6. OrderService (Log)
   INFO|OrderService|abc-123|Creating order in database

7. OrderService → Saga (RabbitMQ)
   OrderCreatedEvent
   CorrelationId: abc-123
   
8. Saga → StockService (RabbitMQ)
   OrderCreatedEvent
   CorrelationId: abc-123
   
9. StockService (Log)
   INFO|StockService|abc-123|Reserving stock for order

10. StockService → Saga (RabbitMQ)
    StockReservedEvent
    CorrelationId: abc-123

... ve böyle devam eder ...

Tüm bu adımlarda aynı "abc-123" Correlation ID kullanılır!
```

### Log Sorgulama Örneği:

**Konsol/Text loglarında:**
```bash
# Belirli bir Correlation ID'nin tüm loglarını bul
grep "abc-123" logs/*.log

# Belirli bir servisin logları
grep "abc-123" logs/OrderService-*.log
```

**JSON loglarında:**
```bash
# jq ile JSON logları sorgula
cat logs/OrderService-2026-01-09.json | jq 'select(.correlationId == "abc-123")'

# Sadece error logları
cat logs/*.json | jq 'select(.correlationId == "abc-123" and .level == "ERROR")'
```

---

## 💡 Örnek Senaryolar

### Senaryo 1: Başarılı Sipariş Akışı

**Nasıl Test Edilir:**
```bash
# 1. Basket'e ürün ekle
curl -X POST "http://localhost:5153/api/basket/add" \
  -H "Content-Type: application/json" \
  -d '{"productId": 1, "quantity": 2}'

# 2. Checkout yap ve Correlation ID'yi al
curl -X POST "http://localhost:5153/api/basket/checkout" \
  -H "X-Correlation-ID: test-order-001" \
  -v

# 3. Tüm servislerin loglarında "test-order-001" ara
grep "test-order-001" logs/*/*.log
```

**Beklenen Loglar:**
```
BasketService-all-2026-01-09.log:
  INFO|BasketService|test-order-001|Checkout started

SagaStateMachine-all-2026-01-09.log:
  INFO|SagaStateMachine|test-order-001|Order started event received
  INFO|SagaStateMachine|test-order-001|State changed to OrderCreated

OrderService-all-2026-01-09.log:
  INFO|OrderService|test-order-001|Creating order
  INFO|OrderService|test-order-001|Order created successfully

StockService-all-2026-01-09.log:
  INFO|StockService|test-order-001|Reserving stock
  INFO|StockService|test-order-001|Stock reserved successfully

PaymentService-all-2026-01-09.log:
  INFO|PaymentService|test-order-001|Processing payment
  INFO|PaymentService|test-order-001|Payment completed

DeliveryService-all-2026-01-09.log:
  INFO|DeliveryService|test-order-001|Creating delivery
  INFO|DeliveryService|test-order-001|Delivery scheduled
```

### Senaryo 2: Stok Yetersiz Hatası

**Test:**
```bash
curl -X POST "http://localhost:5153/api/basket/checkout" \
  -H "X-Correlation-ID: test-insufficient-stock" \
  -d '{"items": [{"productId": 999, "quantity": 1000}]}'
```

**Beklenen Loglar:**
```
StockService-all-2026-01-09.log:
  WARN|StockService|test-insufficient-stock|Insufficient stock for product 999
  INFO|StockService|test-insufficient-stock|Publishing StockNotReservedEvent

SagaStateMachine-all-2026-01-09.log:
  WARN|SagaStateMachine|test-insufficient-stock|Stock reservation failed
  INFO|SagaStateMachine|test-insufficient-stock|Publishing OrderFailedEvent
```

---

## 🔍 Troubleshooting

### Problem 1: Correlation ID log'larda görünmüyor

**Çözüm:**
```bash
# 1. nlog.config'de MDLC kullanıldığından emin olun:
${mdlc:item=CorrelationId}

# 2. Middleware'in doğru sırada olduğunu kontrol edin (Program.cs):
app.UseCorrelationId();  // Bu satır diğer middleware'lerden ÖNCE olmalı
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
```

### Problem 2: Consumer'larda Correlation ID yok

**Çözüm:**
```csharp
// ServiceRegistration.cs'de global filter eklendiğinden emin olun:
cfg.UsingRabbitMq((context, hostConfig) =>
{
    hostConfig.Host(configuration.GetConnectionString("RabbitMq"));
    
    // Bu satır OLMALIDIR:
    hostConfig.UseConsumeFilter(typeof(MassTransitCorrelationFilter<>), context);
    
    // Endpoint konfigürasyonları...
});
```

### Problem 3: Log dosyaları oluşmuyor

**Çözüm:**
```bash
# 1. logs klasörünün yazma izni olduğunu kontrol edin
chmod 755 logs/

# 2. Internal NLog log'larını kontrol edin
cat logs/internal-nlog-ServiceName.txt

# 3. nlog.config dosyasının doğru konumda olduğunu doğrulayın
# (ServiceName.WebApi klasörünün kök dizininde olmalı)
```

### Problem 4: Correlation ID servisler arası kayboluy or

**Çözüm:**
MassTransit mesajları gönderirken CorrelationId'nin aktarıldığından emin olun:

```csharp
// Event publish ederken
await _publishEndpoint.Publish(new OrderCreatedEvent
{
    OrderId = order.Id,
    // ... diğer alanlar
}, context =>
{
    // Mevcut CorrelationId'yi aktar
    context.CorrelationId = Guid.Parse(currentCorrelationId);
});
```

---

## 🚀 İleri Seviye Kullanım

### Seq Entegrasyonu

`nlog.config` dosyasına ekleyin:

```xml
<targets>
  <target name="seq" xsi:type="Seq" serverUrl="http://localhost:5341">
    <property name="CorrelationId" value="${mdlc:item=CorrelationId}" />
    <property name="Service" value="${serviceName}" />
  </target>
</targets>

<rules>
  <logger name="*" minlevel="Info" writeTo="seq" />
</rules>
```

NuGet paketi:
```bash
dotnet add package NLog.Targets.Seq
```

### ElasticSearch Entegrasyonu

```xml
<targets>
  <target name="elastic" xsi:type="ElasticSearch" 
          uri="http://localhost:9200" 
          index="ecommerce-logs-${date:format=yyyy.MM.dd}"
          includeAllProperties="true">
    <field name="correlationId" layout="${mdlc:item=CorrelationId}" />
    <field name="service" layout="${serviceName}" />
  </target>
</targets>
```

NuGet paketi:
```bash
dotnet add package NLog.Targets.ElasticSearch
```

---

## 📝 Özet

Bu implementasyon ile:
- ✅ Tüm mikroservisler yapılandırılmış loglama yapıyor
- ✅ Her request benzersiz bir Correlation ID ile izlenebiliyor
- ✅ MassTransit mesajları Correlation ID'yi taşıyor
- ✅ Consumer'lar otomatik olarak Correlation ID'yi loglara ekliyor
- ✅ Log dosyaları hem text hem JSON formatında kaydediliyor
- ✅ Production'da merkezi log toplama (Seq, ELK) için hazır

**Geliştiriciler için:** Artık bir hatayı veya request'i tüm mikroservisler boyunca tek bir Correlation ID ile takip edebilirsiniz!

---

## 📚 Ek Kaynaklar

- [NLog Documentation](https://nlog-project.org/)
- [MassTransit Correlation](https://masstransit.io/documentation/concepts/messages#correlation)
- [Distributed Tracing Best Practices](https://www.w3.org/TR/trace-context/)
