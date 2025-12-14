using System;
using System.Data.SQLite;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TelefonSatısApp
{
    public partial class PeşinSatisPenceresi : Window
    {
        private Telefon _telefon;
        private bool _isFormattingPrice;
        private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("tr-TR");

        public PeşinSatisPenceresi(Telefon telefon)
        {
            InitializeComponent();

            _telefon = telefon;

            // Telefon bilgilerini formda göster
            TxtModel.Text = _telefon.Model;
            TxtAlinanFiyat.Text = $"{_telefon.AlinanFiyat.GetValueOrDefault(0).ToString("N0", _culture)} ₺";
        }

        private void Button_SatisTamamla(object sender, RoutedEventArgs e)
        {
            var musteriAdSoyad = TxtMusteriAdSoyad.Text.Trim();
            var musteriTelefon = TxtMusteriTelefon.Text;

            // Satış fiyatı doğrulaması
            if (!TryParsePrice(TxtSatisFiyati.Text, out double satisFiyati))
            {
                var dialog = new TemaliMesajPenceresi("Uyarı", "Geçerli bir satış fiyatı girin.");
                dialog.Owner = this;
                dialog.ShowDialog();
                return;
            }

            if (string.IsNullOrEmpty(musteriAdSoyad) || string.IsNullOrEmpty(musteriTelefon))
            {
                var dialog = new TemaliMesajPenceresi("Uyarı", "Müşteri bilgilerini doldurun.");
                dialog.Owner = this;
                dialog.ShowDialog();
                return;
            }

            // Kar hesaplama
            double kar = satisFiyati - _telefon.AlinanFiyat.GetValueOrDefault(0); // nullable double için kontrol

            // Satışı kaydet
            using (var conn = Database.GetConnection())
            {
                conn.Open();

                // Peşin satış verilerini kaydet
                string insertPesinSatisSql = @"
            INSERT INTO PesinSatislar (TelefonId, MusteriAd, MusteriSoyad, MusteriAdSoyad, MusteriTelefon, Marka, Model, Durum, SatisFiyati, Kar, AlinanFiyat, Tarih) 
            VALUES (@TelefonId, @MusteriAd, @MusteriSoyad, @MusteriAdSoyad, @MusteriTelefon, @Marka, @Model, @Durum, @SatisFiyati, @Kar, @AlinanFiyat, @Tarih);
        ";

                using (var cmd = new SQLiteCommand(insertPesinSatisSql, conn))
                {
                    // Eski alanları da doldur (geriye uyumluluk için)
                    var parts = musteriAdSoyad.Split(' ', 2);
                    var ad = parts.Length > 0 ? parts[0] : "";
                    var soyad = parts.Length > 1 ? parts[1] : "";

                    cmd.Parameters.AddWithValue("@TelefonId", _telefon.Id);
                    cmd.Parameters.AddWithValue("@MusteriAd", ad);
                    cmd.Parameters.AddWithValue("@MusteriSoyad", soyad);
                    cmd.Parameters.AddWithValue("@MusteriAdSoyad", musteriAdSoyad);
                    cmd.Parameters.AddWithValue("@MusteriTelefon", musteriTelefon);
                    cmd.Parameters.AddWithValue("@Marka", _telefon.Marka ?? "");
                    cmd.Parameters.AddWithValue("@Model", _telefon.Model ?? "");
                    cmd.Parameters.AddWithValue("@Durum", _telefon.Durum ?? "");
                    cmd.Parameters.AddWithValue("@SatisFiyati", satisFiyati);
                    cmd.Parameters.AddWithValue("@Kar", kar);
                    cmd.Parameters.AddWithValue("@AlinanFiyat", _telefon.AlinanFiyat.GetValueOrDefault(0));
                    cmd.Parameters.AddWithValue("@Tarih", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }

                // Satılan telefonu veritabanından silme
                string deletePhoneSql = "DELETE FROM Telefonlar WHERE Imei = @Imei"; // Telefonu IMEI'ye göre siliyoruz
                using (var deleteCmd = new SQLiteCommand(deletePhoneSql, conn))
                {
                    deleteCmd.Parameters.AddWithValue("@Imei", _telefon.Imei); // Satılan telefonun IMEI'sini alıyoruz
                    deleteCmd.ExecuteNonQuery();
                }
            }

            var successDialog = new TemaliMesajPenceresi("Başarılı", "Satış işlemi tamamlandı ve telefon veritabanından silindi!");
            successDialog.Owner = this;
            successDialog.ShowDialog();

            // Pencereyi kapat
            this.Close();
        }

        private bool TryParsePrice(string input, out double value)
        {
            // Nokta/virgül ayracını temizleyerek sadece rakamları parse ediyoruz.
            var normalized = input.Replace(".", "").Replace(",", "");
            return double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private void TxtSatisFiyati_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormattingPrice) return;

            try
            {
                _isFormattingPrice = true;
                if (TryParsePrice(TxtSatisFiyati.Text, out double value))
                {
                    TxtSatisFiyati.Text = value.ToString("N0", _culture);
                    TxtSatisFiyati.CaretIndex = TxtSatisFiyati.Text.Length;
                    UpdateKar(value);
                }
                else
                {
                    TxtKar.Text = "-";
                }
            }
            finally
            {
                _isFormattingPrice = false;
            }
        }

        private void TxtSatisFiyati_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Sadece rakam girişine izin ver
            foreach (var ch in e.Text)
            {
                if (!char.IsDigit(ch))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private void UpdateKar(double satisFiyati)
        {
            var kar = satisFiyati - _telefon.AlinanFiyat.GetValueOrDefault(0);
            TxtKar.Text = $"{kar.ToString("N0", _culture)} ₺";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }



}
