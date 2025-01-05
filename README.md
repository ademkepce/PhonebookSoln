# Mikroservis Yapıları

Bu proje, mikroservis mimarisiyle tasarlanmış bir uygulamadır. Her bir mikroservis, belirli bir işlevi yerine getiren bağımsız bir birim olarak çalışır. Mikroservisler, kendi veritabanlarına sahip olabilir, bağımsız bir şekilde deploy edilebilir ve birbirleriyle API çağrıları ve mesaj kuyruğu (Message Queue) gibi yöntemlerle iletişim kurar.

**PhonebookSoln.Infrastructure** projesindeki veritabanı migration işlemlerini uygulayın:

dotnet ef migrations add InitialCreate -p PhonebookSoln.Presentation
dotnet ef database update -p PhonebookSoln.Presentation

## Outbox Design Pattern

Outbox Design Pattern, mikroservisler arasındaki veri tutarsızlıklarını önlemek ve asenkron işlem süreçlerini yönetmek için kullanılan bir desendir. Özellikle bir mikroservisin başka bir mikroservise veri gönderdiği durumlarda, bu verinin kaybolması veya çift işleme gibi sorunları önlemeye yönelik bir yaklaşımdır. Bu projede, Outbox Design Pattern'ı mikroservisler arası güvenli veri iletimi sağlamak için kullanılmıştır.

### **Outbox Design Pattern Kullanımı**

Outbox Design Pattern, bir mikroservisin işlem yaptığı veriyi doğrudan dışa aktarmadan önce, bu veriyi bir "outbox" tablosunda saklamayı içerir. İşlem başarılı olduğunda, mikroservis öncelikle bu veriyi outbox tablosuna kaydeder. Ardından, asenkron bir süreç (örneğin, bir mesaj kuyruğu veya arka planda çalışan bir iş) bu veriyi dış sisteme veya başka bir mikroservise iletir. Bu yaklaşım, verinin doğru bir şekilde dışa aktarıldığından emin olmak için kullanılabilir.

#### **Uygulama Adımları**

1. **Outbox Tablosu**: 
   - Projeye eklenen bir outbox tablosu, gönderilecek mesajları geçici olarak saklar.
   - Bu tablo, mesajın gönderilme durumunu (başarılı, başarısız, vb.) takip etmek için kullanılır.

2. **Transaction Management**: 
   - İşlem yapıldığında (örneğin, bir kişi kaydedildiğinde), bu işlem bir transaction içerisinde gerçekleştirilir. Hem veritabanına veri kaydedilir, hem de outbox tablosuna bir mesaj eklenir.
   - Transaction'un başarılı bir şekilde tamamlanması durumunda, mesaj dışarıya iletilir.

3. **Asenkron İşlem**: 
   - Arka planda çalışan bir servis, outbox tablosundaki mesajları belirli aralıklarla kontrol eder.
   - Mesajlar başarıyla iletildiğinde, outbox tablosundaki durum güncellenir ve mesajlar silinir.

#### **Outbox Tablosu Yapısı**

Outbox tablosu genellikle aşağıdaki gibi bir yapıya sahip olur:

- **Id**: Mesajın benzersiz kimliği
- **Payload**: Gönderilecek veri
- **Status**: Mesajın iletilip iletilmediğini belirten durum bilgisi (e.g. Pending, Sent)
- **CreatedAt**: Mesajın oluşturulma zamanı
- **SentAt**: Mesajın dışa gönderildiği zaman (Eğer gönderildiyse)
  
#### **Kullanım Senaryosu**

- **Kişi Kaydetme**: Kullanıcı yeni bir kişi eklemek için API çağrısı yaptığında, bu işlemle ilgili veriler önce veritabanına kaydedilir ve aynı zamanda outbox tablosuna da bir mesaj eklenir.
- **Mesaj Gönderimi**: Arka planda çalışan bir servis, outbox tablosunu kontrol ederek mesajı ilgili mikroservise (örneğin RabbitMQ aracılığıyla) gönderir. Mesaj başarılı bir şekilde gönderildiğinde, outbox tablosundaki durum güncellenir ve mesaj silinir.
  
Bu desen sayesinde, mikroservisler arası veri iletimi güvenli hale gelir ve bir işlem başarıyla gerçekleşse bile mesaj kaybolmaz veya iki kez iletilmez. Aynı zamanda, veri tutarlılığını sağlamak için işlem ve dışa gönderme süreçleri arasında sıkı bir ilişki kurulur.

#### **Örnek Uygulama: ContactService'de Outbox Kullanımı**

- `ContactService`'te bir kişi ekleme işlemi yapılırken, bu işlemle ilgili veriler önce veritabanına kaydedilir, ardından bir outbox kaydı oluşturulur.
- Arka planda çalışan bir servis (örneğin bir `OutboxProcessor`), outbox tablosunu belirli aralıklarla kontrol eder. Eğer mesajın durumu "Pending" ise, bu mesaj RabbitMQ üzerinden ilgili mikroservise gönderilir. Mesaj gönderildikten sonra, outbox kaydının durumu güncellenir.

Bu yaklaşım, tüm mikroservislerde veri kayıplarını önlemeye yardımcı olur ve işlemlerin güvenilir bir şekilde gerçekleştirilmesini sağlar.

## Proje Yapısı

Aşağıda mikroservisler için yapıların genel bir görünümü verilmiştir:

- **PhonebookSoln.Presentation (API Katmanı)**: Kullanıcı ile etkileşimde bulunan, HTTP isteklerine yanıt veren katman.
- **PhonebookSoln.Application (İş Mantığı Katmanı)**: Uygulama iş mantığını yöneten, servisleri barındıran katman.
- **PhonebookSoln.Core (Ortak Veri Yapıları)**: Tüm mikroservislerin kullandığı ortak modeller ve sınıflar.
- **PhonebookSoln.Infrastructure (Veritabanı ve Dış Servis Entegrasyonları)**: Veritabanı işlemleri ve dış sistemlere bağlantıları yöneten katman.
- **PhonebookSoln.Tests (Test Katmanı)**: Unit testler ve entegrasyon testlerini içeren katman.

## Mikroservisler

### **1. ContactService**

`ContactService`, telefon rehberindeki kişilerin yönetimi için kullanılan bir mikroservistir. Kişilerle ilgili işlemleri yöneten ana servis olarak, kişilerin eklenmesi, silinmesi, güncellenmesi ve listeleme işlemleri yapılır.

#### Endpointler:

- **POST /api/contacts**  
  **İşlev**: Yeni bir kişi ekler.  
  **Nasıl Çalışır**:  
  - Kullanıcıdan gelen veriler `ContactDto` modeline aktarılır.
  - Veritabanına yeni kişi eklenir ve işlem başarılı ise yanıt döner.
  
- **DELETE /api/contacts/{id}**  
  **İşlev**: Kişiyi rehberden siler.  
  **Nasıl Çalışır**:  
  - Kişi ID'si ile ilgili silme işlemi yapılır ve kullanıcıya başarılı yanıt döner.

- **GET /api/contacts**  
  **İşlev**: Rehberdeki tüm kişileri listeler.  
  **Nasıl Çalışır**:  
  - Veritabanından tüm kişiler alınır ve kullanıcıya döner.

- **GET /api/contacts/{id}**  
  **İşlev**: Belirli bir kişinin detaylarını getirir.  
  **Nasıl Çalışır**:  
  - Kullanıcı tarafından belirtilen kişi ID'sine göre detaylar sorgulanır ve döner.

#### Bağlantılar:

- **Veritabanı**: MSSQL
- **Uygulama Servisi**: `ContactService` iş mantığını içerir ve `ContactController` aracılığıyla API'den erişilebilir.

---

### **2. ReportService**

`ReportService`, kullanıcıların rapor taleplerini işleyen mikroservistir. Raporlar, belirli kriterlere göre toplanan verileri asenkron olarak işlemekte ve kullanıcılara sunmaktadır.

#### Endpointler:

- **POST /api/reports**  
  **İşlev**: Yeni bir rapor talebi alır.  
  **Nasıl Çalışır**:  
  - Kullanıcı, rapor için gerekli kriterleri belirtir.
  - RabbitMQ üzerinden mesaj kuyruğuna rapor oluşturma isteği gönderilir.
  - Rapor oluşturma işlemi arka planda başlatılır.

- **GET /api/reports**  
  **İşlev**: Kullanıcının talepleriyle oluşturulmuş tüm raporları listeler.  
  **Nasıl Çalışır**:  
  - Veritabanındaki raporlar sorgulanır ve kullanıcıya döner.

- **GET /api/reports/{id}**  
  **İşlev**: Belirli bir raporun detayını getirir.  
  **Nasıl Çalışır**:  
  - Raporun ID'sine göre detaylı bilgi sorgulanır.

#### Bağlantılar:

- **Veritabanı**: MSSQL
- **Mesajlaşma Sistemi**: RabbitMQ
- **Uygulama Servisi**: `ReportService` iş mantığını içerir ve `ReportController` aracılığıyla API'den erişilebilir.

---

## Uygulama Katmanları

### **PhonebookSoln.Presentation (API Katmanı)**

Bu katman, tüm dış talepleri karşılayan API katmanıdır. Controller'lar aracılığıyla kullanıcılardan gelen HTTP istekleri alınır ve ilgili servislere yönlendirilir.

#### Örnek Akış:

1. Kullanıcı **POST /api/contacts** isteği gönderir.
2. Bu istek, `ContactController`'a gelir.
3. `ContactController`, `ContactService`'i çağırır ve veritabanına ekleme işlemi yapılır.
4. Son olarak, işlem başarılı ise kullanıcıya yanıt döner.

### **PhonebookSoln.Application (İş Mantığı Katmanı)**

İş mantığı katmanı, mikroservislerin core mantığını içerir. Buradaki servisler, gelen istekleri işler ve veritabanı ya da başka bir dış sisteme yönlendirir.

#### **ContactService**:

- Kişi ekleme, silme, güncelleme ve listeleme işlemleri burada yapılır.
- Servis, veritabanına doğrudan erişim sağlar.

#### **ReportService**:

- Rapor oluşturma, durum takibi ve rapor sonuçlarını almak burada işlenir.
- RabbitMQ kullanarak asenkron bir şekilde raporlar arka planda oluşturulur.

### **PhonebookSoln.Infrastructure (Veritabanı ve Dış Servis Entegrasyonları)**

Veritabanı işlemleri bu katmanda yer alır. Burada, kişilerin ve raporların veritabanına kaydedilmesi ve dış sistemlerle entegrasyon yapılması sağlanır.

#### **Entity Framework / MSSQL**:

- Kişi ve rapor bilgileri bu katmandan yönetilen veritabanlarına kaydedilir.

---

## İletişim Yöntemleri

Mikroservisler arası iletişim için aşağıdaki yöntemler kullanılır:

- **HTTP API**: Controller'lar aracılığıyla doğrudan HTTP istekleri ile iletişim sağlanır.
- **Message Queue (RabbitMQ)**: Asenkron işlemler için, özellikle rapor oluşturma gibi zaman alıcı işlemler RabbitMQ üzerinden yapılır.
