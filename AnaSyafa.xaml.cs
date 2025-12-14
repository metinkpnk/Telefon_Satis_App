using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Win32;
using ClosedXML.Excel;

namespace TelefonSatısApp
{
    /// <summary>
    /// Hatırlatma öğelerini temsil eden sınıf - bugün vadesi gelen taksitler için kullanılır
    /// </summary>
    public class HatirlatmaItem
    {
        /// <summary>Müşterinin adı ve soyadı</summary>
        public string MusteriAdSoyad { get; set; } = "";
        /// <summary>Müşterinin telefon numarası</summary>
        public string Telefon { get; set; } = "";
        /// <summary>Taksitin vade tarihi</summary>
        public DateTime VadeTarihi { get; set; }
    }

    /// <summary>
    /// Ana sayfa sınıfı - telefon listesi, hatırlatmalar ve temel işlemleri yönetir
    /// </summary>
    public partial class AnaSyafa : Page
    {
        /// <summary>Ekranda gösterilen filtrelenmiş telefon listesi</summary>
        public ObservableCollection<Telefon> Telefonlar { get; set; }
        /// <summary>Tüm telefonları içeren ana liste - arama işlemleri için kullanılır</summary>
        private ObservableCollection<Telefon> TumTelefonlar { get; set; }
        /// <summary>Bugün vadesi gelen taksit hatırlatmaları</summary>
        public ObservableCollection<HatirlatmaItem> Hatirlatmalar { get; set; } = new();
        /// <summary>Saat gösterimini güncellemek için kullanılan zamanlayıcı</summary>
        private readonly DispatcherTimer _saatTimer = new DispatcherTimer();

        /// <summary>
        /// Ana sayfa yapıcı metodu - sayfa yüklendiğinde çalışır
        /// </summary>
        public AnaSyafa()
        {
            InitializeComponent();
            Telefonlar = new ObservableCollection<Telefon>();
            TumTelefonlar = new ObservableCollection<Telefon>();
            LoadTelefonlar();
            LoadHatirlatmalar();
            SaatBaslat();
        }

        /// <summary>
        /// Veritabanından telefon verilerini yükler ve DataGrid'e bağlar
        /// </summary>
        private void LoadTelefonlar()
        {
            Telefonlar.Clear();
            TumTelefonlar.Clear();

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "SELECT Id, Imei, Marka, Model, Renk, GarantiAy, CikisYili, AlinanFiyat, Durum FROM Telefonlar";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string durumRaw = reader.IsDBNull(8) ? "" : reader.GetString(8);
                        string durum = NormalizeDurum(durumRaw);
                        var telefon = new Telefon
                        {
                            Id = reader.GetInt32(0),
                            Imei = reader.GetString(1),
                            Marka = reader.GetString(2),
                            Model = reader.GetString(3),
                            Renk = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            GarantiAy = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            CikisYili = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                            AlinanFiyat = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
                            Durum = durum
                        };
                        TumTelefonlar.Add(telefon);
                        Telefonlar.Add(telefon);
                    }
                }
            }

            // DataGrid'e telefonları aktar
            DataGridTelefonlar.ItemsSource = Telefonlar;
        }

        /// <summary>
        /// Bugün vadesi gelen taksit ödemelerini yükler ve hatırlatma listesine ekler
        /// </summary>
        private void LoadHatirlatmalar()
        {
            Hatirlatmalar.Clear();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT s.MusteriAd, s.MusteriSoyad, s.Telefon1, s.Telefon2, t.VadeTarihi
                    FROM TaksitOdemeleri t
                    INNER JOIN TaksitliSatislar s ON s.Id = t.TaksitliSatisId
                    WHERE date(t.VadeTarihi) = date(@Bugun) AND t.Odendi = 0
                    ORDER BY t.VadeTarihi;";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Bugun", DateTime.Now.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string ad = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            string soyad = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            string tel1 = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            string tel2 = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            var vade = reader.GetDateTime(4);

                            Hatirlatmalar.Add(new HatirlatmaItem
                            {
                                MusteriAdSoyad = $"{ad} {soyad}".Trim(),
                                Telefon = string.IsNullOrWhiteSpace(tel2) ? tel1 : $"{tel1} / {tel2}",
                                VadeTarihi = vade
                            });
                        }
                    }
                }
            }

            ItemsHatirlatma.ItemsSource = Hatirlatmalar;
        }

        /// <summary>
        /// Telefon durumunu standart formata çevirir (Sıfır/İkinci El)
        /// </summary>
        /// <param name="raw">Ham durum metni</param>
        /// <returns>Normalize edilmiş durum</returns>
        private static string NormalizeDurum(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Sıfır";

            var txt = raw.Trim().ToLowerInvariant();
            if (txt.Contains("ikinci") || txt.Contains("2"))
                return "İkinci El";

            return "Sıfır";
        }

        /// <summary>
        /// DataGrid'de seçili olan telefonu döndürür
        /// </summary>
        /// <returns>Seçili telefon nesnesi veya null</returns>
        private Telefon? SeciliTelefon()
        {
            return DataGridTelefonlar.SelectedItem as Telefon;
        }

        /// <summary>
        /// Yeni telefon ekleme penceresini açar
        /// </summary>
        private void Button_Ekle(object sender, RoutedEventArgs e)
        {
            var pencere = new TelefonEkle();
            bool? sonuc = pencere.ShowDialog();

            if (sonuc == true)
            {
                LoadTelefonlar();  // Tabloyu yeniler
            }
        }

        /// <summary>
        /// Seçili telefonu güncelleme penceresini açar
        /// </summary>
        private void Button_Güncelle(object sender, RoutedEventArgs e)
        {
            var secili = SeciliTelefon();
            if (secili == null)
            {
                var dialog = new TemaliMesajPenceresi("Uyarı", "Lütfen güncellenecek telefonu seç.");
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                return;
            }

            var pencere = new TelefonGuncelle(secili);
            bool? sonuc = pencere.ShowDialog();

            if (sonuc == true)
                LoadTelefonlar();
        }

        /// <summary>
        /// Seçili telefonu veritabanından siler
        /// </summary>
        private void Button_Sil(object sender, RoutedEventArgs e)
        {
            var secili = SeciliTelefon();
            if (secili == null)
            {
                var dialog = new TemaliMesajPenceresi("Uyarı", "Lütfen listeden bir telefon seç.");
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                return;
            }

            var onay = new SilOnayPenceresi("Telefonu Sil", "Bu telefonu silmek istediğine emin misin?");
            onay.Owner = Window.GetWindow(this);
            if (onay.ShowDialog() != true)
                return;

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM Telefonlar WHERE Id = @Id";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", secili.Id);
                    cmd.ExecuteNonQuery();
                }
            }

            LoadTelefonlar();
            var successDialog = new TemaliMesajPenceresi("Başarılı", "Telefon başarıyla silindi.");
            successDialog.Owner = Window.GetWindow(this);
            successDialog.ShowDialog();
        }

        /// <summary>
        /// Seçili telefon için peşin satış penceresini açar
        /// </summary>
        private void Button_PeşinSatış(object sender, RoutedEventArgs e)
        {
            var seciliTelefon = DataGridTelefonlar.SelectedItem as Telefon;

            if (seciliTelefon == null)
            {
                var dialog = new TemaliMesajPenceresi("Uyarı", "Lütfen satış yapılacak telefonu seçin.");
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                return;
            }

            // Peşin satış penceresini aç
            var peşinSatışPenceresi = new PeşinSatisPenceresi(seciliTelefon);
            peşinSatışPenceresi.ShowDialog();

            // Tabloyu güncelle
            LoadTelefonlar(); // Telefonlar listesine tekrar bak
        }

        /// <summary>
        /// Telefon listesini ve hatırlatmaları yeniler
        /// </summary>
        private void Button_Yenile(object sender, RoutedEventArgs e)
        {
            LoadTelefonlar();
            LoadHatirlatmalar();
        }

        /// <summary>
        /// Seçili telefon için taksitli satış penceresini açar
        /// </summary>
        private void Button_TaksitliSatış(object sender, RoutedEventArgs e)
        {
            var seciliTelefon = DataGridTelefonlar.SelectedItem as Telefon;
            if (seciliTelefon == null)
            {
                var dialog = new TemaliMesajPenceresi("Uyarı", "Lütfen satış yapılacak telefonu seçin.");
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
                return;
            }

            var pencere = new TaksitliSatisPenceresi(seciliTelefon);
            bool? sonuc = pencere.ShowDialog();

            if (sonuc == true)
            {
                LoadTelefonlar();
            }
        }

        /// <summary>
        /// Telefon listesini Excel dosyasına aktarır
        /// </summary>
        private void Button_ExcelExport(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Telefonlar.Count == 0)
                {
                    var dialog = new TemaliMesajPenceresi("Uyarı", "Dışa aktarılacak telefon bulunamadı.");
                    dialog.Owner = Window.GetWindow(this);
                    dialog.ShowDialog();
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Dosyaları (*.xlsx)|*.xlsx|Tüm Dosyalar (*.*)|*.*",
                    FileName = $"Telefon_Listesi_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    DefaultExt = "xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Telefonlar");

                        // Başlık satırı
                        worksheet.Cell(1, 1).Value = "IMEI";
                        worksheet.Cell(1, 2).Value = "Marka";
                        worksheet.Cell(1, 3).Value = "Model";
                        worksheet.Cell(1, 4).Value = "Durum";
                        worksheet.Cell(1, 5).Value = "Renk";
                        worksheet.Cell(1, 6).Value = "Garanti (Ay)";
                        worksheet.Cell(1, 7).Value = "Çıkış Yılı";
                        worksheet.Cell(1, 8).Value = "Alınan Fiyat";

                        // Başlık stilini ayarla
                        var headerRange = worksheet.Range(1, 1, 1, 8);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                        headerRange.Style.Font.FontColor = XLColor.White;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Veri satırları
                        int row = 2;
                        var trCulture = CultureInfo.GetCultureInfo("tr-TR");
                        foreach (var telefon in Telefonlar)
                        {
                            worksheet.Cell(row, 1).Value = telefon.Imei ?? "";
                            worksheet.Cell(row, 2).Value = telefon.Marka ?? "";
                            worksheet.Cell(row, 3).Value = telefon.Model ?? "";
                            worksheet.Cell(row, 4).Value = telefon.Durum ?? "";
                            worksheet.Cell(row, 5).Value = telefon.Renk ?? "";
                            worksheet.Cell(row, 6).Value = telefon.GarantiAy ?? 0;
                            worksheet.Cell(row, 7).Value = telefon.CikisYili ?? 0;
                            
                            if (telefon.AlinanFiyat.HasValue)
                            {
                                worksheet.Cell(row, 8).Value = telefon.AlinanFiyat.Value;
                                worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
                            }
                            else
                            {
                                worksheet.Cell(row, 8).Value = "";
                            }

                            row++;
                        }

                        // Sütun genişliklerini ayarla
                        worksheet.Column(1).Width = 18;
                        worksheet.Column(2).Width = 15;
                        worksheet.Column(3).Width = 18;
                        worksheet.Column(4).Width = 12;
                        worksheet.Column(5).Width = 12;
                        worksheet.Column(6).Width = 12;
                        worksheet.Column(7).Width = 12;
                        worksheet.Column(8).Width = 15;

                        // Tabloyu otomatik filtrele
                        worksheet.Range(1, 1, row - 1, 8).SetAutoFilter();

                        workbook.SaveAs(saveDialog.FileName);
                    }

                    var successDialog = new TemaliMesajPenceresi("Başarılı", $"Telefon listesi başarıyla Excel dosyasına aktarıldı.\n\nDosya: {Path.GetFileName(saveDialog.FileName)}");
                    successDialog.Owner = Window.GetWindow(this);
                    successDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var errorDialog = new TemaliMesajPenceresi("Hata", $"Excel dosyası oluşturulurken hata oluştu:\n\n{ex.Message}");
                errorDialog.Owner = Window.GetWindow(this);
                errorDialog.ShowDialog();
            }
        }

        /// <summary>
        /// Saat gösterimini başlatır ve her saniye günceller
        /// </summary>
        private void SaatBaslat()
        {
            _saatTimer.Interval = TimeSpan.FromSeconds(1);
            _saatTimer.Tick += (s, e) =>
            {
                ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
            _saatTimer.Start();
        }

        /// <summary>
        /// Arama kutusundaki metne göre telefon listesini filtreler
        /// IMEI, Marka ve Model alanlarında arama yapar
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string aramaMetni = SearchBox.Text?.Trim().ToLowerInvariant() ?? "";
            
            Telefonlar.Clear();
            
            if (string.IsNullOrEmpty(aramaMetni))
            {
                // Arama metni boşsa tüm telefonları göster
                foreach (var telefon in TumTelefonlar)
                {
                    Telefonlar.Add(telefon);
                }
            }
            else
            {
                // Arama metnine göre filtrele (IMEI, Marka, Model)
                var filtreliTelefonlar = TumTelefonlar.Where(t => 
                    (t.Imei?.ToLowerInvariant().Contains(aramaMetni) == true) ||
                    (t.Marka?.ToLowerInvariant().Contains(aramaMetni) == true) ||
                    (t.Model?.ToLowerInvariant().Contains(aramaMetni) == true));
                
                foreach (var telefon in filtreliTelefonlar)
                {
                    Telefonlar.Add(telefon);
                }
            }
        }
    }
}
