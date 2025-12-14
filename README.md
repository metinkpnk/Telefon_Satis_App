# 📱 Telefon Satış Uygulaması

Modern ve kullanıcı dostu bir telefon satış yönetim sistemi. Bu uygulama, telefon satış işletmeleri için envanter yönetimi, satış takibi, taksit yönetimi ve gelir-gider analizi gibi temel işlevleri sunar.

## 🚀 Özellikler

### 📋 Envanter Yönetimi
- **Telefon Ekleme/Düzenleme/Silme**: Detaylı telefon bilgileri (IMEI, marka, model, renk, garanti süresi)
- **Akıllı Arama**: IMEI, marka ve model bazında hızlı arama
- **Excel Dışa Aktarım**: Envanter listesini Excel formatında dışa aktarma
- **Durum Takibi**: Sıfır/İkinci el telefon durumu yönetimi

### 💰 Satış Yönetimi
- **Peşin Satış**: Anında ödeme ile satış işlemleri
- **Taksitli Satış**: Esnek taksit planları ile satış
- **Müşteri Bilgileri**: Detaylı müşteri kayıt sistemi
- **Satış Geçmişi**: Tüm satış işlemlerinin takibi

### 📅 Taksit Takip Sistemi
- **Ödeme Takibi**: Taksit ödemelerinin durumu ve vade tarihleri
- **Hatırlatmalar**: Bugün vadesi gelen ödemeler için otomatik hatırlatma
- **Not Sistemi**: Her müşteri için özel notlar
- **Ödeme İşaretleme**: Tek tıkla ödeme durumu güncelleme

### 📊 Gelir-Gider Analizi
- **Detaylı Raporlar**: Dönemsel gelir-gider analizi
- **Kar Hesaplama**: Otomatik kar marjı hesaplama
- **Grafik Gösterimler**: Görsel analiz araçları
- **Dışa Aktarım**: Raporları CSV formatında kaydetme

### 🔔 Bildirim Sistemi
- **Vade Takibi**: Bugün vadesi gelen ödemeler
- **Geciken Taksitler**: Vadesi geçmiş ödemeler
- **Özet Bilgiler**: Hızlı durum özeti

## 🖥️ Ekran Görüntüleri

### Ana Sayfa
Ana sayfa, telefon envanterini görüntülemenizi ve yönetmenizi sağlar:
- Telefon listesi ve arama özelliği
- Hızlı işlem butonları (Ekle, Güncelle, Sil)
- Peşin ve taksitli satış seçenekleri
- Bugünkü hatırlatmalar paneli
- Canlı saat gösterimi

### Telefon Ekleme/Düzenleme
Yeni telefon ekleme veya mevcut telefon bilgilerini güncelleme:
- IMEI numarası girişi
- Marka ve model seçimi (önceden tanımlı listeler)
- Renk, garanti süresi ve çıkış yılı bilgileri
- Alış fiyatı ve durum bilgisi

### Satış İşlemleri
#### Peşin Satış
- Müşteri bilgileri girişi
- Satış fiyatı belirleme
- Kar hesaplama
- Anında satış tamamlama

#### Taksitli Satış
- Taksit sayısı belirleme
- Ön ödeme tutarı
- Aylık ödeme hesaplama
- Taksit takvimi oluşturma

### Taksit Takip
Taksitli satışların yönetimi:
- Müşteri bazında taksit görüntüleme
- Ödeme durumu güncelleme
- Not ekleme ve düzenleme
- Arama ve filtreleme

### Bildirimler
Ödeme hatırlatmaları ve durum bilgileri:
- Bugünkü ödemeler listesi
- Geciken taksitler
- Yaklaşan vadeler
- Özet istatistikler

### Gelir-Gider Raporu
Finansal analiz araçları:
- Dönemsel gelir analizi
- Gider takibi
- Kar marjı hesaplama
- Detaylı raporlama

## 🛠️ Teknik Özellikler

### Teknoloji Stack
- **Framework**: .NET WPF (Windows Presentation Foundation)
- **Dil**: C# 
- **Veritabanı**: SQLite
- **UI Framework**: Modern WPF tasarımı
- **Excel İşlemleri**: ClosedXML kütüphanesi

### Sistem Gereksinimleri
- **İşletim Sistemi**: Windows 10 veya üzeri
- **.NET Framework**: .NET 6.0 veya üzeri
- **RAM**: Minimum 4GB
- **Disk Alanı**: 100MB boş alan

### Veritabanı Yapısı
```sql
-- Telefonlar tablosu
Telefonlar (Id, Imei, Marka, Model, Renk, GarantiAy, CikisYili, AlinanFiyat, Durum)

-- Peşin satışlar
PesinSatislar (Id, MusteriAd, MusteriSoyad, MusteriTelefon, Marka, Model, SatisFiyati, Kar, Tarih)

-- Taksitli satışlar
TaksitliSatislar (Id, TelefonId, MusteriAd, MusteriSoyad, Telefon1, Telefon2, TaksitSayisi, SatisFiyati, OnOdeme, AylikOdeme, Tarih)

-- Taksit ödemeleri
TaksitOdemeleri (Id, TaksitliSatisId, TaksitNo, VadeTarihi, Odendi, OdemeTarihi)
```

## 📦 Kurulum

### Geliştirici Kurulumu
1. **Projeyi klonlayın**:
   ```bash
   git clone [repository-url]
   cd TelefonSatısApp
   ```

2. **Gerekli paketleri yükleyin**:
   ```bash
   dotnet restore
   ```

3. **Uygulamayı çalıştırın**:
   ```bash
   dotnet run
   ```

### Son Kullanıcı Kurulumu
1. `publish` klasöründen setup dosyasını indirin
2. Setup dosyasını çalıştırın
3. Kurulum talimatlarını takip edin
4. Uygulamayı başlatın

## 🎯 Kullanım Kılavuzu

### İlk Kurulum
1. Uygulamayı ilk kez açtığınızda veritabanı otomatik olarak oluşturulur
2. Ana sayfada telefon ekleme işlemini başlatabilirsiniz
3. Temel ayarları yapılandırın

### Telefon Ekleme
1. Ana sayfada "Telefon Ekle" butonuna tıklayın
2. IMEI numarasını girin
3. Marka ve model seçin
4. Diğer bilgileri doldurun
5. "Kaydet" butonuna tıklayın

### Satış İşlemi
1. Satılacak telefonu listeden seçin
2. "Peşin Satış" veya "Taksitli Satış" butonuna tıklayın
3. Müşteri bilgilerini girin
4. Satış koşullarını belirleyin
5. Satışı tamamlayın

### Taksit Takibi
1. "Taksit Takip" sayfasına gidin
2. Müşteri listesinden ilgili kişiyi bulun
3. Taksit butonlarına tıklayarak ödeme durumunu güncelleyin
4. Gerekirse not ekleyin

## 🔧 Yapılandırma

### Veritabanı Ayarları
Uygulama SQLite veritabanı kullanır ve otomatik olarak yapılandırılır. Veritabanı dosyası uygulama klasöründe `telefon_satis.db` adıyla saklanır.

### Yedekleme
Düzenli olarak veritabanı dosyasını yedeklemeniz önerilir:
- Veritabanı konumu: `[Uygulama Klasörü]/telefon_satis.db`
- Manuel yedekleme: Dosyayı kopyalayın
- Otomatik yedekleme: Gelecek sürümlerde eklenecek

## 🤝 Katkıda Bulunma

Bu proje açık kaynak değildir, ancak önerilerinizi ve geri bildirimlerinizi memnuniyetle karşılarız.

### Hata Bildirimi
Hata bulduğunuzda lütfen aşağıdaki bilgileri paylaşın:
- Hata açıklaması
- Adım adım tekrar etme yöntemi
- Ekran görüntüleri (varsa)
- Sistem bilgileri

## 📞 Destek

### Teknik Destek
- **E-posta**: [destek-email]
- **Telefon**: [destek-telefon]
- **Çalışma Saatleri**: Pazartesi-Cuma 09:00-18:00

### Sık Sorulan Sorular

**S: Veritabanım bozuldu, ne yapmalıyım?**
A: Yedek dosyanızı geri yükleyin veya teknik destek ile iletişime geçin.

**S: Excel dışa aktarım çalışmıyor?**
A: Microsoft Excel'in yüklü olduğundan emin olun veya CSV formatını kullanın.

**S: Taksit hesaplamaları yanlış görünüyor?**
A: Satış fiyatı ve ön ödeme tutarlarını kontrol edin, gerekirse satışı düzenleyin.

## 📄 Lisans

Bu yazılım ticari bir üründür. Kullanım koşulları için lisans sözleşmesini inceleyiniz.

## 🔄 Sürüm Geçmişi

### v1.0.0 (Mevcut)
- ✅ Temel envanter yönetimi
- ✅ Peşin ve taksitli satış
- ✅ Taksit takip sistemi
- ✅ Bildirim sistemi
- ✅ Gelir-gider raporları
- ✅ Excel dışa aktarım

### Gelecek Sürümler
- 🔄 Otomatik yedekleme sistemi
- 🔄 SMS hatırlatma entegrasyonu
- 🔄 Gelişmiş raporlama
- 🔄 Çoklu kullanıcı desteği
- 🔄 Bulut senkronizasyonu

## 📊 İstatistikler

- **Kod Satırı**: ~5000+ satır
- **Dosya Sayısı**: 25+ dosya
- **Özellik Sayısı**: 15+ ana özellik
- **Desteklenen Format**: Excel, CSV

---

**© 2024 Telefon Satış Uygulaması. Tüm hakları saklıdır.**