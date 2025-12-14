using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TelefonSatısApp
{
    /// <summary>
    /// Taksit ödeme görünümü - her bir taksit ödemesini temsil eder
    /// </summary>
    public class TaksitOdemeView
    {
        private static readonly CultureInfo _tr = CultureInfo.GetCultureInfo("tr-TR");
        /// <summary>Taksit ödeme ID'si</summary>
        public int Id { get; set; }
        /// <summary>Bağlı olduğu satış ID'si</summary>
        public int ParentSatisId { get; set; }
        /// <summary>Taksit numarası</summary>
        public int TaksitNo { get; set; }
        /// <summary>Vade tarihi</summary>
        public DateTime VadeTarihi { get; set; }
        /// <summary>Ödenip ödenmediği</summary>
        public bool Odendi { get; set; }
        /// <summary>Taksit tutarı</summary>
        public double Tutar { get; set; }
        /// <summary>Buton üzerinde gösterilecek metin</summary>
        public string ButtonText => $"Taksit {TaksitNo}\n{VadeTarihi:dd.MM.yyyy}\n{Tutar.ToString("N0", _tr)} ₺";
    }

    /// <summary>
    /// Taksitli satış takip görünümü - bir satışın tüm taksitlerini içerir
    /// </summary>
    public class TaksitliSatisTakipView
    {
        /// <summary>Satış ID'si</summary>
        public int SatisId { get; set; }
        /// <summary>Müşteri adı ve soyadı</summary>
        public string MusteriAdSoyad { get; set; } = "";
        /// <summary>Telefon bilgisi</summary>
        public string TelefonBilgi { get; set; } = "";
        /// <summary>Aylık ödeme tutarı</summary>
        public double AylikOdeme { get; set; }
        /// <summary>Notlar</summary>
        public string Notlar { get; set; } = "";
        /// <summary>Kalan ödeme tutarı</summary>
        public double KalanOdeme { get; set; }
        /// <summary>Taksit ödemeleri listesi</summary>
        public ObservableCollection<TaksitOdemeView> Taksitler { get; set; } = new();
    }

    /// <summary>
    /// Taksit takip sayfası - taksitli satışları ve ödemelerini yönetir
    /// </summary>
    public partial class TaksitTakip : Page
    {
        public ObservableCollection<TaksitliSatisTakipView> Satislar { get; set; } = new();
        private ObservableCollection<TaksitliSatisTakipView> TumSatislar { get; set; } = new();

        public TaksitTakip()
        {
            InitializeComponent();
            ItemsSatislar.ItemsSource = Satislar;
            LoadData();
        }

        private void LoadData()
        {
            Satislar.Clear();
            TumSatislar.Clear();

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT s.Id, s.MusteriAd, s.MusteriSoyad, s.Telefon1, s.Telefon2, s.TaksitSayisi, s.Tarih, s.SatisFiyati, s.AylikOdeme, s.Notlar,
                           t.Id, t.TaksitNo, t.VadeTarihi, t.Odendi
                    FROM TaksitliSatislar s
                    LEFT JOIN TaksitOdemeleri t ON t.TaksitliSatisId = s.Id
                    ORDER BY s.Id, t.TaksitNo;";

                var lookup = new Dictionary<int, TaksitliSatisTakipView>();

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int satisId = reader.GetInt32(0);
                        if (!lookup.TryGetValue(satisId, out var satis))
                        {
                            string ad = reader.GetString(1);
                            string soyad = reader.GetString(2);
                            string tel1 = reader.GetString(3);
                            string tel2 = reader.IsDBNull(4) ? "" : reader.GetString(4);
                            double aylik = reader.IsDBNull(8) ? 0 : reader.GetDouble(8);
                            string notlar = reader.IsDBNull(9) ? "" : reader.GetString(9);
                            satis = new TaksitliSatisTakipView
                            {
                                SatisId = satisId,
                                MusteriAdSoyad = $"{ad} {soyad}",
                                TelefonBilgi = string.IsNullOrWhiteSpace(tel2) ? tel1 : $"{tel1} / {tel2}",
                                AylikOdeme = aylik,
                                Notlar = notlar,
                                Taksitler = new ObservableCollection<TaksitOdemeView>()
                            };
                            lookup[satisId] = satis;
                            TumSatislar.Add(satis);
                        }

                        if (!reader.IsDBNull(10))
                        {
                            satis.Taksitler.Add(new TaksitOdemeView
                            {
                                Id = reader.GetInt32(10),
                                ParentSatisId = satis.SatisId,
                                TaksitNo = reader.GetInt32(11),
                                VadeTarihi = reader.GetDateTime(12),
                                Odendi = reader.GetInt32(13) == 1,
                                Tutar = satis.AylikOdeme
                            });
                        }
                    }
                }
            }

            foreach (var satis in TumSatislar)
            {
                satis.KalanOdeme = satis.Taksitler.Where(t => !t.Odendi).Sum(t => t.Tutar);
                Satislar.Add(satis);
            }
        }

        private void NotKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TaksitliSatisTakipView satis)
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("UPDATE TaksitliSatislar SET Notlar = @Notlar WHERE Id = @Id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@Notlar", satis.Notlar ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Id", satis.SatisId);
                        cmd.ExecuteNonQuery();
                    }
                }
                ThemedMessageBox.Show("Not kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TaksitButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaksitOdemeView odeme)
            {
                bool yeniDurum = !odeme.Odendi;

                string mesaj = yeniDurum
                    ? $"Taksit {odeme.TaksitNo} ({odeme.VadeTarihi:dd.MM.yyyy}) {odeme.Tutar.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"))} ₺ ödensin mi?"
                    : $"Taksit {odeme.TaksitNo} ödenmedi olarak işaretlensin mi?";
                var cevap = ThemedMessageBox.Show(mesaj, "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (cevap != MessageBoxResult.Yes)
                    return;

                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE TaksitOdemeleri SET Odendi = @Odendi, OdemeTarihi = @OdemeTarihi WHERE Id = @Id;";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Odendi", yeniDurum ? 1 : 0);
                        cmd.Parameters.AddWithValue("@OdemeTarihi", yeniDurum ? (object)DateTime.Now : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Id", odeme.Id);
                        cmd.ExecuteNonQuery();
                    }
                }

                odeme.Odendi = yeniDurum;
                var parentSatis = Satislar.FirstOrDefault(s => s.SatisId == odeme.ParentSatisId);
                if (parentSatis != null)
                {
                    parentSatis.KalanOdeme = parentSatis.Taksitler.Where(t => !t.Odendi || (t.Id == odeme.Id && !yeniDurum)).Sum(t => t.Tutar);
                }

                // Hepsi ödendiyse satışın silinmesini sor
                bool hepsiOdendi = false;
                var parent = Satislar.FirstOrDefault(s => s.SatisId == odeme.ParentSatisId);
                if (parent != null)
                {
                    hepsiOdendi = parent.Taksitler.All(t => t.Odendi || t.Id == odeme.Id && yeniDurum);
                }

                if (hepsiOdendi)
                {
                    var silCevap = ThemedMessageBox.Show("Tüm taksitler ödendi. Bu satışı silmek ister misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (silCevap == MessageBoxResult.Yes)
                    {
                        using (var conn = Database.GetConnection())
                        {
                            conn.Open();
                            using (var cmd = new SQLiteCommand("DELETE FROM TaksitOdemeleri WHERE TaksitliSatisId = @Id;", conn))
                            {
                                cmd.Parameters.AddWithValue("@Id", odeme.ParentSatisId);
                                cmd.ExecuteNonQuery();
                            }
                            using (var cmd = new SQLiteCommand("DELETE FROM TaksitliSatislar WHERE Id = @Id;", conn))
                            {
                                cmd.Parameters.AddWithValue("@Id", odeme.ParentSatisId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                LoadData(); // yenile
            }
        }

        private void AramaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string aramaMetni = AramaTextBox.Text?.Trim().ToLowerInvariant() ?? "";
            
            Satislar.Clear();
            
            if (string.IsNullOrEmpty(aramaMetni))
            {
                // Arama metni boşsa tüm satışları göster
                foreach (var satis in TumSatislar)
                {
                    Satislar.Add(satis);
                }
            }
            else
            {
                // Arama metnine göre filtrele
                var filtreliSatislar = TumSatislar.Where(s => 
                    s.MusteriAdSoyad.ToLowerInvariant().Contains(aramaMetni));
                
                foreach (var satis in filtreliSatislar)
                {
                    Satislar.Add(satis);
                }
            }
        }

        private void TemizleButton_Click(object sender, RoutedEventArgs e)
        {
            AramaTextBox.Text = "";
            AramaTextBox.Focus();
        }

        private void Button_Yenile(object sender, RoutedEventArgs e)
        {
            LoadData();
            ThemedMessageBox.Show("Taksit takip verileri yenilendi!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_ExcelExport(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    Title = "CSV Dosyası Kaydet",
                    FileName = $"TaksitTakip_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = "Müşteri Adı Soyadı,Telefon Bilgisi,Kalan Ödeme,Notlar,Taksit No,Vade Tarihi,Tutar,Ödendi\n";
                    
                    foreach (var satis in TumSatislar)
                    {
                        foreach (var taksit in satis.Taksitler)
                        {
                            csv += $"\"{satis.MusteriAdSoyad}\",\"{satis.TelefonBilgi}\",\"{satis.KalanOdeme:N0}\",\"{satis.Notlar}\",\"{taksit.TaksitNo}\",\"{taksit.VadeTarihi:dd.MM.yyyy}\",\"{taksit.Tutar:N0}\",\"{(taksit.Odendi ? "Evet" : "Hayır")}\"\n";
                        }
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csv, System.Text.Encoding.UTF8);
                    
                    ThemedMessageBox.Show($"Taksit takip verileri başarıyla dışa aktarıldı!\nDosya: {saveFileDialog.FileName}", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show($"Dışa aktarım sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

