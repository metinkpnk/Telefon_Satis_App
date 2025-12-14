using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TelefonSatısApp
{
    /// <summary>
    /// Bildirim öğelerini temsil eden sınıf - taksit ödemeleri için kullanılır
    /// </summary>
    public class BildirimItem
    {
        /// <summary>Müşterinin adı ve soyadı</summary>
        public string MusteriAdi { get; set; } = "";
        /// <summary>Taksit detay bilgisi (taksit no, telefon, vade tarihi)</summary>
        public string Detay { get; set; } = "";
        /// <summary>Ödeme tutarı</summary>
        public string Tutar { get; set; } = "";
        /// <summary>Durum ikonu (emoji)</summary>
        public string Icon { get; set; } = "";
        /// <summary>Durum metni (BUGÜN, GECİKEN, YAKLAŞAN)</summary>
        public string DurumText { get; set; } = "";
        /// <summary>Arka plan rengi</summary>
        public SolidColorBrush BackgroundColor { get; set; } = new SolidColorBrush(Color.FromRgb(15, 23, 42));
        /// <summary>Kenarlık rengi</summary>
        public SolidColorBrush BorderColor { get; set; } = new SolidColorBrush(Color.FromRgb(30, 41, 59));
        /// <summary>Durum metni rengi</summary>
        public SolidColorBrush DurumColor { get; set; } = new SolidColorBrush(Color.FromRgb(37, 99, 235));
        /// <summary>Vade tarihi</summary>
        public DateTime VadeTarihi { get; set; }
        /// <summary>Bugün vadesi gelen mi?</summary>
        public bool IsBugun { get; set; }
        /// <summary>Vadesi geçmiş mi?</summary>
        public bool IsGeciken { get; set; }
    }

    /// <summary>
    /// Bildirimler sayfası - taksit ödemelerini takip eder ve hatırlatmalar gösterir
    /// </summary>
    public partial class Bildirimler : Page
    {
        /// <summary>Tüm bildirimleri içeren ana liste</summary>
        private ObservableCollection<BildirimItem> _tumBildirimler = new();
        /// <summary>Filtrelenmiş bildirimleri içeren liste (ekranda gösterilen)</summary>
        private ObservableCollection<BildirimItem> _filtreliBildirimler = new();

        /// <summary>
        /// Bildirimler sayfası yapıcı metodu
        /// </summary>
        public Bildirimler()
        {
            InitializeComponent();
            BildirimListesi.ItemsSource = _filtreliBildirimler;
            LoadData();
        }

        /// <summary>
        /// Veritabanından ödenmemiş taksit bilgilerini yükler ve bildirim listesini oluşturur
        /// </summary>
        private void LoadData()
        {
            _tumBildirimler.Clear();
            
            var bugun = DateTime.Today;
            var tr = CultureInfo.GetCultureInfo("tr-TR");

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT 
                        s.MusteriAd, s.MusteriSoyad, s.Telefon1,
                        t.TaksitNo, t.VadeTarihi, t.Odendi, s.AylikOdeme
                    FROM TaksitliSatislar s
                    INNER JOIN TaksitOdemeleri t ON t.TaksitliSatisId = s.Id
                    WHERE t.Odendi = 0
                    ORDER BY t.VadeTarihi ASC";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var musteriAd = reader.GetString(0);
                        var musteriSoyad = reader.GetString(1);
                        var telefon = reader.GetString(2);
                        var taksitNo = reader.GetInt32(3);
                        var vadeTarihi = reader.GetDateTime(4);
                        var aylikOdeme = reader.IsDBNull(6) ? 0 : reader.GetDouble(6);

                        var isBugun = vadeTarihi.Date == bugun;
                        var isGeciken = vadeTarihi.Date < bugun;

                        var bildirim = new BildirimItem
                        {
                            MusteriAdi = $"{musteriAd} {musteriSoyad}",
                            Detay = $"Taksit {taksitNo} • {telefon} • Vade: {vadeTarihi:dd.MM.yyyy}",
                            Tutar = $"{aylikOdeme:N0} ₺",
                            VadeTarihi = vadeTarihi,
                            IsBugun = isBugun,
                            IsGeciken = isGeciken
                        };

                        if (isBugun)
                        {
                            bildirim.Icon = "📅";
                            bildirim.DurumText = "BUGÜN";
                            bildirim.DurumColor = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Warning
                            bildirim.BackgroundColor = new SolidColorBrush(Color.FromRgb(20, 16, 8)); // Dark warning bg
                            bildirim.BorderColor = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Warning border
                        }
                        else if (isGeciken)
                        {
                            bildirim.Icon = "⚠️";
                            bildirim.DurumText = "GECİKEN";
                            bildirim.DurumColor = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Error
                            bildirim.BackgroundColor = new SolidColorBrush(Color.FromRgb(20, 8, 8)); // Dark error bg
                            bildirim.BorderColor = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Error border
                        }
                        else
                        {
                            bildirim.Icon = "🔔";
                            bildirim.DurumText = "YAKLAŞAN";
                            bildirim.DurumColor = new SolidColorBrush(Color.FromRgb(37, 99, 235)); // Primary
                            bildirim.BackgroundColor = new SolidColorBrush(Color.FromRgb(15, 23, 42)); // Default bg
                            bildirim.BorderColor = new SolidColorBrush(Color.FromRgb(30, 41, 59)); // Default border
                        }

                        _tumBildirimler.Add(bildirim);
                    }
                }
            }

            UpdateSummary();
            ShowAllNotifications();
        }

        /// <summary>
        /// Özet bilgilerini günceller (bugünkü ödemeler, geciken taksitler, geciken tutar)
        /// </summary>
        private void UpdateSummary()
        {
            var bugun = DateTime.Today;
            
            var bugunkuOdemeler = _tumBildirimler.Count(b => b.IsBugun);
            var gecikenTaksitler = _tumBildirimler.Count(b => b.IsGeciken);
            var gecikenTutar = _tumBildirimler
                .Where(b => b.IsGeciken)
                .Sum(b => {
                    var tutarStr = b.Tutar.Replace(" ₺", "").Replace(".", "");
                    return double.TryParse(tutarStr, out double tutar) ? tutar : 0;
                });

            TxtBugunkuOdemeler.Text = bugunkuOdemeler.ToString();
            TxtGecikenTaksitler.Text = gecikenTaksitler.ToString();
            TxtGecikenTutar.Text = $"{gecikenTutar:N0} ₺";
        }

        /// <summary>
        /// Sadece bugünkü ödemeleri gösterir
        /// </summary>
        private void BtnBugunku_Click(object sender, RoutedEventArgs e)
        {
            ShowTodayNotifications();
        }

        /// <summary>
        /// Sadece geciken taksitleri gösterir
        /// </summary>
        private void BtnGeciken_Click(object sender, RoutedEventArgs e)
        {
            ShowOverdueNotifications();
        }

        /// <summary>
        /// Tüm bildirimleri gösterir
        /// </summary>
        private void BtnTumunu_Click(object sender, RoutedEventArgs e)
        {
            ShowAllNotifications();
        }

        /// <summary>
        /// Sadece bugün vadesi gelen ödemeleri filtreler ve gösterir
        /// </summary>
        private void ShowTodayNotifications()
        {
            _filtreliBildirimler.Clear();
            var bugunkuBildirimler = _tumBildirimler.Where(b => b.IsBugun).ToList();
            
            foreach (var bildirim in bugunkuBildirimler)
            {
                _filtreliBildirimler.Add(bildirim);
            }

            TxtListeBaslik.Text = $"Bugünkü Ödemeler ({bugunkuBildirimler.Count})";
            
            // Buton renklerini güncelle
            BtnBugunku.Background = new SolidColorBrush(Color.FromRgb(245, 158, 11));
            BtnGeciken.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            BtnTumunu.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
        }

        /// <summary>
        /// Sadece vadesi geçmiş taksitleri filtreler ve gösterir
        /// </summary>
        private void ShowOverdueNotifications()
        {
            _filtreliBildirimler.Clear();
            var gecikenBildirimler = _tumBildirimler.Where(b => b.IsGeciken).ToList();
            
            foreach (var bildirim in gecikenBildirimler)
            {
                _filtreliBildirimler.Add(bildirim);
            }

            TxtListeBaslik.Text = $"Geciken Taksitler ({gecikenBildirimler.Count})";
            
            // Buton renklerini güncelle
            BtnBugunku.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            BtnGeciken.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            BtnTumunu.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
        }

        /// <summary>
        /// Tüm bildirimleri gösterir (filtre kaldırır)
        /// </summary>
        private void ShowAllNotifications()
        {
            _filtreliBildirimler.Clear();
            
            foreach (var bildirim in _tumBildirimler)
            {
                _filtreliBildirimler.Add(bildirim);
            }

            TxtListeBaslik.Text = $"Tüm Bildirimler ({_tumBildirimler.Count})";
            
            // Buton renklerini güncelle
            BtnBugunku.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            BtnGeciken.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            BtnTumunu.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129));
        }

        /// <summary>
        /// Bildirim verilerini yeniden yükler
        /// </summary>
        private void BtnYenile_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            ThemedMessageBox.Show("Bildirimler yenilendi!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Bildirimleri Excel dosyasına aktarır
        /// </summary>
        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExportToExcel();
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show($"Excel'e aktarım sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Bildirim verilerini CSV formatında dışa aktarır
        /// </summary>
        private void ExportToExcel()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Excel Dosyası Kaydet",
                FileName = $"Bildirimler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Excel export işlemi burada yapılacak
                // Şimdilik basit bir CSV formatında kaydetme
                var csv = "Müşteri Adı,Detay,Tutar,Durum,Vade Tarihi\n";
                
                foreach (var bildirim in _tumBildirimler)
                {
                    var durum = bildirim.IsBugun ? "BUGÜN" : (bildirim.IsGeciken ? "GECİKEN" : "YAKLAŞAN");
                    csv += $"\"{bildirim.MusteriAdi}\",\"{bildirim.Detay}\",\"{bildirim.Tutar}\",\"{durum}\",\"{bildirim.VadeTarihi:dd.MM.yyyy}\"\n";
                }

                System.IO.File.WriteAllText(saveFileDialog.FileName.Replace(".xlsx", ".csv"), csv, System.Text.Encoding.UTF8);
                ThemedMessageBox.Show($"Bildirimler başarıyla dışa aktarıldı!\nDosya: {saveFileDialog.FileName.Replace(".xlsx", ".csv")}", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}