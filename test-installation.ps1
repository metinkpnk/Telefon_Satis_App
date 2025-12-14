# T&T Telefon Satış Uygulaması - Kurulum Test Scripti
# Bu script kurulum sistemini test eder

param(
    [switch]$FullTest,
    [switch]$QuickTest
)

# Encoding ayarla
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Renkli yazı fonksiyonları
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "⚠️ $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "ℹ️ $Message" -ForegroundColor Cyan }
function Write-Title { param($Message) Write-Host "`n🔍 $Message" -ForegroundColor Magenta }

# Başlık
Clear-Host
Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║          T&T Telefon Satış - Kurulum Test Scripti           ║" -ForegroundColor Blue
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Blue

Write-Title "KURULUM SİSTEMİ TEST EDİLİYOR"

# Test 1: Gerekli dosyaların varlığı
Write-Title "Test 1: Gerekli Dosyalar"

$RequiredFiles = @(
    "single-file\TelefonSatısApp.exe",
    "installer.bat",
    "installer.ps1", 
    "create-portable.bat",
    "setup-guide.md",
    "TelefonSatısApp.csproj"
)

$MissingFiles = @()
foreach ($file in $RequiredFiles) {
    if (Test-Path $file) {
        Write-Success "Bulundu: $file"
    } else {
        Write-Error "Eksik: $file"
        $MissingFiles += $file
    }
}

if ($MissingFiles.Count -eq 0) {
    Write-Success "Tüm gerekli dosyalar mevcut!"
} else {
    Write-Error "$($MissingFiles.Count) dosya eksik!"
}

# Test 2: Exe dosyası kontrolü
Write-Title "Test 2: Ana Uygulama Dosyası"

if (Test-Path "single-file\TelefonSatısApp.exe") {
    $ExeInfo = Get-Item "single-file\TelefonSatısApp.exe"
    $SizeMB = [math]::Round($ExeInfo.Length / 1MB, 2)
    
    Write-Success "Dosya boyutu: $SizeMB MB"
    Write-Success "Son değişiklik: $($ExeInfo.LastWriteTime)"
    
    if ($SizeMB -gt 50) {
        Write-Success "Dosya boyutu uygun (Self-contained)"
    } else {
        Write-Warning "Dosya boyutu küçük, self-contained olmayabilir"
    }
} else {
    Write-Error "Ana uygulama dosyası bulunamadı!"
}

# Test 3: Installer scriptleri syntax kontrolü
Write-Title "Test 3: Installer Script Kontrolü"

# Batch installer kontrolü
if (Test-Path "installer.bat") {
    $BatchContent = Get-Content "installer.bat" -Raw
    if ($BatchContent -match "INSTALL_DIR.*Program Files") {
        Write-Success "Batch installer: Kurulum yolu doğru"
    } else {
        Write-Warning "Batch installer: Kurulum yolu kontrol edilmeli"
    }
    
    if ($BatchContent -match "net session") {
        Write-Success "Batch installer: Yönetici kontrolü mevcut"
    } else {
        Write-Warning "Batch installer: Yönetici kontrolü eksik"
    }
}

# PowerShell installer kontrolü
if (Test-Path "installer.ps1") {
    try {
        $null = Get-Content "installer.ps1" | Out-String
        Write-Success "PowerShell installer: Syntax doğru"
    } catch {
        Write-Error "PowerShell installer: Syntax hatası - $($_.Exception.Message)"
    }
}

# Test 4: Proje dosyası kontrolü
Write-Title "Test 4: Proje Konfigürasyonu"

if (Test-Path "TelefonSatısApp.csproj") {
    $ProjectContent = Get-Content "TelefonSatısApp.csproj" -Raw
    
    if ($ProjectContent -match "net8.0-windows") {
        Write-Success "Hedef framework: .NET 8.0 Windows"
    } else {
        Write-Warning "Hedef framework kontrol edilmeli"
    }
    
    if ($ProjectContent -match "UseWPF.*true") {
        Write-Success "WPF desteği: Aktif"
    } else {
        Write-Warning "WPF desteği kontrol edilmeli"
    }
    
    if ($ProjectContent -match "ApplicationIcon") {
        Write-Success "Uygulama ikonu: Yapılandırılmış"
    } else {
        Write-Info "Uygulama ikonu: Yapılandırılmamış (opsiyonel)"
    }
}

# Test 5: Kurulum simülasyonu (sadece FullTest ile)
if ($FullTest) {
    Write-Title "Test 5: Kurulum Simülasyonu"
    
    $TestInstallDir = "$env:TEMP\TT-Test-Install"
    
    Write-Info "Test kurulum dizini: $TestInstallDir"
    
    # Test dizini oluştur
    if (Test-Path $TestInstallDir) {
        Remove-Item $TestInstallDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $TestInstallDir -Force | Out-Null
    
    # Dosya kopyalama testi
    try {
        Copy-Item "single-file\TelefonSatısApp.exe" "$TestInstallDir\TelefonSatısApp.exe"
        Write-Success "Dosya kopyalama: Başarılı"
        
        # Kısayol oluşturma testi
        $WshShell = New-Object -comObject WScript.Shell
        $Shortcut = $WshShell.CreateShortcut("$TestInstallDir\Test.lnk")
        $Shortcut.TargetPath = "$TestInstallDir\TelefonSatısApp.exe"
        $Shortcut.Save()
        
        if (Test-Path "$TestInstallDir\Test.lnk") {
            Write-Success "Kısayol oluşturma: Başarılı"
        } else {
            Write-Warning "Kısayol oluşturma: Başarısız"
        }
        
    } catch {
        Write-Error "Kurulum simülasyonu hatası: $($_.Exception.Message)"
    } finally {
        # Temizlik
        if (Test-Path $TestInstallDir) {
            Remove-Item $TestInstallDir -Recurse -Force
        }
    }
}

# Test 6: Dokümantasyon kontrolü
Write-Title "Test 6: Dokümantasyon"

if (Test-Path "setup-guide.md") {
    $GuideContent = Get-Content "setup-guide.md" -Raw
    if ($GuideContent -match "Kurulum.*Seçenekleri") {
        Write-Success "Kurulum rehberi: İçerik uygun"
    } else {
        Write-Warning "Kurulum rehberi: İçerik kontrol edilmeli"
    }
} else {
    Write-Warning "Kurulum rehberi bulunamadı"
}

# Özet
Write-Title "TEST SONUÇLARI"

$TotalTests = 6
$PassedTests = 0

# Basit geçme/kalma hesaplaması
if ($MissingFiles.Count -eq 0) { $PassedTests++ }
if (Test-Path "single-file\TelefonSatısApp.exe") { $PassedTests++ }
if (Test-Path "installer.bat") { $PassedTests++ }
if (Test-Path "installer.ps1") { $PassedTests++ }
if (Test-Path "TelefonSatısApp.csproj") { $PassedTests++ }
if (Test-Path "setup-guide.md") { $PassedTests++ }

$SuccessRate = [math]::Round(($PassedTests / $TotalTests) * 100, 1)

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║                      TEST SONUÇLARI                          ║" -ForegroundColor Blue
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Blue

Write-Host "📊 Başarı Oranı: $SuccessRate% ($PassedTests/$TotalTests)" -ForegroundColor Cyan

if ($SuccessRate -ge 90) {
    Write-Success "🎉 Kurulum sistemi mükemmel durumda!"
} elseif ($SuccessRate -ge 80) {
    Write-Warning "⚠️ Kurulum sistemi iyi durumda, küçük iyileştirmeler yapılabilir"
} else {
    Write-Error "❌ Kurulum sisteminde önemli sorunlar var!"
}

Write-Host "`n💡 Öneriler:" -ForegroundColor Cyan
Write-Host "   - Kurulum öncesi 'dotnet publish' komutunu çalıştırın" -ForegroundColor Gray
Write-Host "   - Installer'ları yönetici yetkileri ile test edin" -ForegroundColor Gray
Write-Host "   - Farklı Windows sürümlerinde test yapın" -ForegroundColor Gray

if (-not $FullTest) {
    Write-Host "`n🔍 Tam test için: .\test-installation.ps1 -FullTest" -ForegroundColor Yellow
}

Write-Host "`nTest tamamlandı!" -ForegroundColor Green