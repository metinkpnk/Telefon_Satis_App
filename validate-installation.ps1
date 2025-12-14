# T&T Telefon Satış Uygulaması - Kurulum Doğrulama Scripti

# Encoding ayarla
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Başlık
Clear-Host
Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║          T&T Telefon Satış - Kurulum Doğrulama              ║" -ForegroundColor Blue
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Blue

Write-Host "`n🔍 Kurulum sistemi kontrol ediliyor..." -ForegroundColor Cyan

# Test 1: Ana dosyalar
Write-Host "`n📋 Gerekli Dosyalar:" -ForegroundColor Yellow

$files = @(
    "single-file\TelefonSatısApp.exe",
    "installer.bat", 
    "installer.ps1",
    "create-portable.bat",
    "setup-guide.md"
)

$passed = 0
foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "  ✅ $file" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "  ❌ $file" -ForegroundColor Red
    }
}

# Test 2: Exe dosyası detayları
Write-Host "`n📱 Ana Uygulama:" -ForegroundColor Yellow

if (Test-Path "single-file\TelefonSatısApp.exe") {
    $exe = Get-Item "single-file\TelefonSatısApp.exe"
    $sizeMB = [math]::Round($exe.Length / 1MB, 1)
    Write-Host "  ✅ Boyut: $sizeMB MB" -ForegroundColor Green
    Write-Host "  ✅ Tarih: $($exe.LastWriteTime)" -ForegroundColor Green
    $passed++
} else {
    Write-Host "  ❌ TelefonSatısApp.exe bulunamadı!" -ForegroundColor Red
}

# Test 3: Proje konfigürasyonu
Write-Host "`n⚙️ Proje Konfigürasyonu:" -ForegroundColor Yellow

if (Test-Path "TelefonSatısApp.csproj") {
    $project = Get-Content "TelefonSatısApp.csproj" -Raw
    
    if ($project -match "net8.0-windows") {
        Write-Host "  ✅ .NET 8.0 Windows" -ForegroundColor Green
        $passed++
    }
    
    if ($project -match "UseWPF.*true") {
        Write-Host "  ✅ WPF Desteği" -ForegroundColor Green
        $passed++
    }
    
    if ($project -match "ApplicationIcon") {
        Write-Host "  ✅ Uygulama İkonu" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ İkon yapılandırılmamış" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ❌ Proje dosyası bulunamadı!" -ForegroundColor Red
}

# Sonuç
Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║                      SONUÇ                                   ║" -ForegroundColor Blue  
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Blue

$total = $files.Count + 3
$percentage = [math]::Round(($passed / $total) * 100, 1)

Write-Host "`n📊 Başarı: $passed/$total (%$percentage)" -ForegroundColor Cyan

if ($percentage -ge 90) {
    Write-Host "🎉 Kurulum sistemi mükemmel!" -ForegroundColor Green
} elseif ($percentage -ge 70) {
    Write-Host "⚠️ Kurulum sistemi iyi durumda" -ForegroundColor Yellow
} else {
    Write-Host "❌ Kurulum sisteminde sorunlar var!" -ForegroundColor Red
}

Write-Host "`n💡 Sonraki adımlar:" -ForegroundColor Cyan
Write-Host "  1. create-distribution.bat ile dağıtım paketi oluşturun" -ForegroundColor Gray
Write-Host "  2. installer.bat ile kurulum test edin" -ForegroundColor Gray
Write-Host "  3. Farklı bilgisayarlarda test yapın" -ForegroundColor Gray

Write-Host "`nDoğrulama tamamlandı!" -ForegroundColor Green