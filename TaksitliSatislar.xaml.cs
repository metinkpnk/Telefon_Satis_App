using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace TelefonSatısApp
{
    /// <summary>
    /// TaksitliSatislar.xaml etkileşim mantığı
    /// </summary>
    public partial class TaksitliSatislar : Page
    {
        public ObservableCollection<TaksitliSatis> Satislar { get; set; } = new ObservableCollection<TaksitliSatis>();
        private ObservableCollection<TaksitliSatis> TumSatislar { get; set; } = new ObservableCollection<TaksitliSatis>();

        public TaksitliSatislar()
        {
            InitializeComponent();
            DataGridTaksitli.ItemsSource = Satislar;
            LoadSatislar();
        }

        private void LoadSatislar()
        {
            Satislar.Clear();
            TumSatislar.Clear();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT ts.Id, ts.TelefonId, ts.MusteriAd, ts.MusteriSoyad, ts.MusteriAdSoyad, 
                           ts.Telefon1, ts.Telefon2, ts.TaksitSayisi, ts.SatisFiyati, ts.OnOdeme, 
                           ts.AylikOdeme, ts.Tarih, ts.TaksitBaslangicTarihi,
                           t.Marka, t.Model, t.Durum
                    FROM TaksitliSatislar ts
                    LEFT JOIN Telefonlar t ON ts.TelefonId = t.Id
                    ORDER BY ts.Tarih DESC";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Birleşik ad soyad varsa onu kullan, yoksa eski alanları birleştir
                        string adSoyad = reader.IsDBNull(4) ? 
                            $"{reader.GetString(2)} {reader.GetString(3)}".Trim() : 
                            reader.GetString(4);

                        var satis = new TaksitliSatis
                        {
                            Id = reader.GetInt32(0),
                            TelefonId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                            MusteriAd = reader.GetString(2),
                            MusteriSoyad = reader.GetString(3),
                            MusteriAdSoyad = adSoyad,
                            Telefon1 = reader.GetString(5),
                            Telefon2 = reader.IsDBNull(6) ? null : reader.GetString(6),
                            TaksitSayisi = reader.GetInt32(7),
                            SatisFiyati = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
                            OnOdeme = reader.IsDBNull(9) ? 0 : reader.GetDouble(9),
                            AylikOdeme = reader.IsDBNull(10) ? 0 : reader.GetDouble(10),
                            Tarih = reader.GetDateTime(11),
                            TaksitBaslangicTarihi = reader.IsDBNull(12) ? reader.GetDateTime(11) : reader.GetDateTime(12),
                            Marka = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            Model = reader.IsDBNull(14) ? "" : reader.GetString(14),
                            Durum = reader.IsDBNull(15) ? "" : reader.GetString(15)
                        };

                        TumSatislar.Add(satis);
                        Satislar.Add(satis);
                    }
                }
            }
        }

        private void Button_SatışıDüzenle(object sender, RoutedEventArgs e)
        {
            if (DataGridTaksitli.SelectedItem is not TaksitliSatis selectedSatis)
            {
                ThemedMessageBox.Show("Lütfen düzenlemek için bir satış seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var duzenlePenceresi = new TaksitliSatisiDuzenle(selectedSatis);
            var sonuc = duzenlePenceresi.ShowDialog();
            if (sonuc == true)
            {
                LoadSatislar();
            }
        }

        private void Button_SatışıSil(object sender, RoutedEventArgs e)
        {
            if (DataGridTaksitli.SelectedItem is not TaksitliSatis selectedSatis)
            {
                ThemedMessageBox.Show("Lütfen silmek için bir satış seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cevap = ThemedMessageBox.Show("Bu satışı ve taksit kayıtlarını silmek istiyor musunuz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (cevap != MessageBoxResult.Yes)
                return;

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM TaksitOdemeleri WHERE TaksitliSatisId = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", selectedSatis.Id);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SQLiteCommand("DELETE FROM TaksitliSatislar WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", selectedSatis.Id);
                    cmd.ExecuteNonQuery();
                }
            }

            LoadSatislar();
        }

        private void Button_Yenile(object sender, RoutedEventArgs e)
        {
            LoadSatislar();
            ThemedMessageBox.Show("Taksitli satışlar yenilendi!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_ExcelExport(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    Title = "CSV Dosyası Kaydet",
                    FileName = $"TaksitliSatislar_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = "Müşteri Adı Soyadı,Marka,Model,Durum,Telefon 1,Telefon 2,Taksit Sayısı,Satış Fiyatı,Ön Ödeme,Aylık Ödeme,Satış Tarihi,Taksit Başlangıç\n";
                    
                    foreach (var satis in Satislar)
                    {
                        csv += $"\"{satis.MusteriAdSoyad}\",\"{satis.Marka}\",\"{satis.Model}\",\"{satis.Durum}\",\"{satis.Telefon1}\",\"{satis.Telefon2}\",\"{satis.TaksitSayisi}\",\"{satis.SatisFiyati:N0}\",\"{satis.OnOdeme:N0}\",\"{satis.AylikOdeme:N0}\",\"{satis.Tarih:dd.MM.yyyy}\",\"{satis.TaksitBaslangicTarihi:dd.MM.yyyy}\"\n";
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csv, System.Text.Encoding.UTF8);
                    
                    ThemedMessageBox.Show($"Taksitli satışlar başarıyla dışa aktarıldı!\nDosya: {saveFileDialog.FileName}", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show($"Dışa aktarım sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AramaTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var aramaMetni = AramaTextBox.Text?.ToLowerInvariant() ?? "";
            
            Satislar.Clear();
            
            if (string.IsNullOrWhiteSpace(aramaMetni))
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
                    s.MusteriAdSoyad.ToLowerInvariant().Contains(aramaMetni) ||
                    s.MusteriAd.ToLowerInvariant().Contains(aramaMetni) ||
                    s.MusteriSoyad.ToLowerInvariant().Contains(aramaMetni)
                );
                
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
    }
}
