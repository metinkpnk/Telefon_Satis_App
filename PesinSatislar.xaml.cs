using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using TelefonSatısApp;

namespace TelefonSatısApp
{
    /// <summary>
    /// PesinSatislar.xaml etkileşim mantığı
    /// </summary>
    public partial class PesinSatislar : Page
    {
        public PesinSatislar()
        {
            InitializeComponent();
            LoadPesinSatislar(); // Peşin satış verilerini yükle
        }

        private void LoadPesinSatislar()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT MusteriAd, MusteriSoyad, MusteriAdSoyad, MusteriTelefon, Marka, Model, Durum, SatisFiyati, Kar, Tarih FROM PesinSatislar";  // Veritabanı sorgusu

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    var satislar = new List<PesinSatis>();  // Veriyi saklamak için liste

                    while (reader.Read())
                    {
                        // Birleşik ad soyad varsa onu kullan, yoksa eski alanları birleştir
                        string adSoyad = reader.IsDBNull(2) ? 
                            $"{reader.GetString(0)} {reader.GetString(1)}".Trim() : 
                            reader.GetString(2);

                        satislar.Add(new PesinSatis
                        {
                            // Nullable alanlar için IsDBNull kontrolü ekliyoruz
                            MusteriAd = reader.GetString(0), // Bu alanda NULL olamaz, direkt çekebiliriz
                            MusteriSoyad = reader.GetString(1), // Bu alanda NULL olamaz, direkt çekebiliriz
                            MusteriAdSoyad = adSoyad,
                            MusteriTelefon = reader.GetString(3), // Bu alanda NULL olamaz, direkt çekebiliriz
                            Marka = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Model = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Durum = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            SatisFiyati = reader.GetDouble(7),  // Bu alanda NULL olamaz, direkt çekebiliriz
                            Kar = reader.GetDouble(8),  // Bu alanda NULL olamaz, direkt çekebiliriz
                            Tarih = reader.IsDBNull(9) ? null : reader.GetDateTime(9)  // Tarih nullable olduğu için kontrol ediyoruz

                        });

                    }

                    // DataGrid'e veri bağlama
                    DataGridPesinSatis.ItemsSource = satislar;

                }
            }

        }

        private void Button_SatışıDüzenle(object sender, RoutedEventArgs e)
        {
            // Seçilen satışı alalım
            var selectedSatis = DataGridPesinSatis.SelectedItem as PesinSatis;

            if (selectedSatis != null)
            {
                // Yeni pencereyi açalım
                PesinSatisiDuzenle satisiDuzenleWindow = new PesinSatisiDuzenle(selectedSatis); // Seçilen satışı yeni pencereye gönderiyoruz
                satisiDuzenleWindow.ShowDialog();

                // Düzenleme işlemi sonrası verileri tekrar yükle
                LoadPesinSatislar();
            }
            else
            {
                var dialog = new TemaliMesajPenceresi("Uyarı", "Lütfen düzenlemek için bir satış seçin.");
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
            }

        }

        private void Button_SatışıSil(object sender, RoutedEventArgs e)
        {
            var selectedSatis = DataGridPesinSatis.SelectedItem as PesinSatis;

            if (selectedSatis != null)
            {
                var dialog = new SilOnayPenceresi("Satışı Sil", "Bu satış silinsin mi?");
                dialog.Owner = Window.GetWindow(this);
                var result = dialog.ShowDialog();

                if (result == true)
                {
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        string sql = "DELETE FROM PesinSatislar WHERE MusteriTelefon = @MusteriTelefon";

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@MusteriTelefon", selectedSatis.MusteriTelefon);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Satışı silindikten sonra verileri tekrar yükle
                    LoadPesinSatislar();
                }
            }
            else
            {
                var dialog = new TemaliMesajPenceresi("Uyarı", "Lütfen silmek için bir satış seçin.");
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
            }

        }

        private void Button_Yenile(object sender, RoutedEventArgs e)
        {
            LoadPesinSatislar();
            var dialog = new TemaliMesajPenceresi("Bilgi", "Peşin satışlar yenilendi!");
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void Button_ExcelExport(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    Title = "CSV Dosyası Kaydet",
                    FileName = $"PesinSatislar_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = "Müşteri Adı Soyadı,Telefon,Marka,Model,Durum,Satış Fiyatı,Kar,Tarih\n";
                    
                    var satislar = DataGridPesinSatis.ItemsSource as List<PesinSatis>;
                    if (satislar != null)
                    {
                        foreach (var satis in satislar)
                        {
                            csv += $"\"{satis.MusteriAdSoyad}\",\"{satis.MusteriTelefon}\",\"{satis.Marka}\",\"{satis.Model}\",\"{satis.Durum}\",\"{satis.SatisFiyati:N0}\",\"{satis.Kar:N0}\",\"{satis.Tarih:dd.MM.yyyy}\"\n";
                        }
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csv, System.Text.Encoding.UTF8);
                    
                    var dialog = new TemaliMesajPenceresi("Başarılı", $"Peşin satışlar başarıyla dışa aktarıldı!\nDosya: {saveFileDialog.FileName}");
                    dialog.Owner = Window.GetWindow(this);
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var dialog = new TemaliMesajPenceresi("Hata", $"Dışa aktarım sırasında hata oluştu: {ex.Message}");
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
            }
        }
    }
}

