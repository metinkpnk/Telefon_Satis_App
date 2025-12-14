# Logo Değiştirme Talimatları

## Logonuzu Eklemek İçin:

1. **Logo dosyanızı hazırlayın:**
   - Dosya formatı: PNG, JPG, veya JPEG
   - Önerilen boyut: 512x512 piksel veya daha büyük (kare format)
   - Şeffaf arka plan (PNG) önerilir

2. **Logo dosyasını kopyalayın:**
   - Logo dosyanızı `Resources` klasörüne kopyalayın
   - Dosya adını `logo.png` olarak değiştirin
   - Mevcut `logo.png` dosyasının üzerine yazın

3. **Proje ayarlarını güncelleyin:**
   - Visual Studio'da `Resources/logo.png` dosyasına sağ tıklayın
   - "Properties" seçin
   - "Build Action" özelliğini "Resource" olarak ayarlayın

## Alternatif Yöntem:

Eğer logo dosyanız farklı bir isimde ise (örneğin `company-logo.png`):

1. `SplashScreen.xaml` dosyasını açın
2. Satır 95'te şu kısmı bulun:
   ```xml
   Source="pack://application:,,,/Resources/logo.png"
   ```
3. `logo.png` kısmını kendi dosya adınızla değiştirin:
   ```xml
   Source="pack://application:,,,/Resources/company-logo.png"
   ```

## Test Etmek İçin:

1. Projeyi yeniden derleyin: `dotnet build`
2. Uygulamayı çalıştırın: `dotnet run`
3. Splash screen'de logonuzun göründüğünü kontrol edin

## Sorun Giderme:

- Logo görünmüyorsa, dosya yolunu ve adını kontrol edin
- Dosya boyutu çok büyükse, resmi küçültmeyi deneyin
- Logo bozuk görünüyorsa, dosya formatını PNG olarak değiştirin

## Özelleştirme:

Splash screen'deki diğer öğeleri de özelleştirebilirsiniz:
- Şirket adı (satır 123)
- Alt yazı (satır 129)
- Renkler ve animasyonlar
- Yükleme mesajları (`SplashScreen.xaml.cs` dosyasında)