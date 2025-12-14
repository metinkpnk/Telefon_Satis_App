# 📦 T&T Telefon Satış Uygulaması - Kurulum Rehberi

## 🎯 Kurulum Seçenekleri

### 1. **Otomatik Kurulum (Önerilen)**

#### **Basit Kurulum (Batch)**
```bash
# Yönetici olarak çalıştır
installer.bat
```

#### **Gelişmiş Kurulum (PowerShell)**
```powershell
# PowerShell'i yönetici olarak aç ve çalıştır
.\installer.ps1
```

### 2. **Manuel Kurulum**
1. `single-file\TelefonSatısApp.exe` dosyasını istediğiniz klasöre kopyalayın
2. Masaüstünde kısayol oluşturun
3. Uygulamayı çalıştırın

---

## 🚀 Kurulum Adımları

### **Hazırlık:**
1. Uygulamayı derleyin:
   ```bash
   dotnet publish --configuration Release --self-contained true --runtime win-x64 --output ./single-file --property:PublishSingleFile=true
   ```

2. Kurulum dosyalarının hazır olduğundan emin olun:
   - ✅ `single-file\TelefonSatısApp.exe`
   - ✅ `installer.bat` veya `installer.ps1`

### **Kurulum:**

#### **Yöntem 1: Batch Installer**
1. `installer.bat` dosyasına sağ tıklayın
2. "Yönetici olarak çalıştır" seçin
3. Kurulum talimatlarını takip edin

#### **Yöntem 2: PowerShell Installer**
1. PowerShell'i yönetici olarak açın
2. Kurulum klasörüne gidin: `cd "C:\path\to\installer"`
3. Scripti çalıştırın: `.\installer.ps1`

---

## 📋 Kurulum Sonrası

### **Kurulum Yerleri:**
- **Uygulama**: `C:\Program Files\TT Kilif Bank\Telefon Satis App\`
- **Masaüstü Kısayolu**: `T&T Telefon Satış.lnk`
- **Başlat Menüsü**: `T&T Telefon Satış`

### **İlk Çalıştırma:**
1. Masaüstündeki kısayola çift tıklayın
2. Uygulama otomatik olarak veritabanını oluşturacak
3. Ana sayfa açılacak ve kullanıma hazır olacak

---

## 🗑️ Kaldırma

### **Otomatik Kaldırma:**
```bash
# Kurulum klasöründe
uninstall.bat

# Veya PowerShell ile
.\uninstall.ps1
```

### **Manuel Kaldırma:**
1. Kurulum klasörünü silin: `C:\Program Files\TT Kilif Bank\`
2. Masaüstü kısayolunu silin
3. Başlat menüsü kısayolunu silin

---

## 🔧 Sorun Giderme

### **Kurulum Hataları:**
- **"Yönetici yetkileri gerekli"**: Installer'ı sağ tıklayıp "Yönetici olarak çalıştır"
- **"Dosya bulunamadı"**: Önce `dotnet publish` komutunu çalıştırın
- **"Kısayol oluşturulamadı"**: Windows Defender veya antivirüs yazılımını kontrol edin

### **Çalıştırma Hataları:**
- **"Uygulama açılmıyor"**: .NET 8.0 Runtime yüklü olduğundan emin olun
- **"Veritabanı hatası"**: Uygulamayı yönetici olarak çalıştırmayı deneyin
- **"Dosya erişim hatası"**: Kurulum klasörüne yazma izni olduğundan emin olun

---

## 📦 Taşınabilir Sürüm

Kurulum yapmadan kullanmak için:
1. `single-file\TelefonSatısApp.exe` dosyasını USB'ye kopyalayın
2. Herhangi bir bilgisayarda çalıştırın
3. Veritabanı dosyaları exe ile aynı klasörde oluşturulacak

---

## 🎯 Sistem Gereksinimleri

- **İşletim Sistemi**: Windows 10/11 (64-bit)
- **RAM**: Minimum 2 GB
- **Disk Alanı**: 200 MB
- **Ekran Çözünürlüğü**: 1024x768 veya üzeri
- **.NET Runtime**: Dahil (self-contained)

---

## 📞 Destek

Kurulum veya kullanım ile ilgili sorunlar için:
- Kurulum loglarını kontrol edin
- Windows Event Viewer'ı inceleyin
- Uygulamayı yönetici olarak çalıştırmayı deneyin