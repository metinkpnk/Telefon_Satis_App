using System;
using System.Data.SQLite;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TelefonSatısApp
{
    /// <summary>
    /// TaksitliSatisiDuzenle.xaml etkileşim mantığı
    /// </summary>
    public partial class TaksitliSatisiDuzenle : Window
    {
        private readonly TaksitliSatis _satis;
        private bool _isFormattingPrice = false;
        private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("tr-TR");

        public TaksitliSatisiDuzenle(TaksitliSatis satis)
        {
            InitializeComponent();
            _satis = satis;

            // Telefon bilgilerini yükle
            LoadPhoneInfo();

            // Birleşik ad soyad kullan, yoksa eski alanları birleştir
            string adSoyad = !string.IsNullOrEmpty(_satis.MusteriAdSoyad) 
                ? _satis.MusteriAdSoyad 
                : $"{_satis.MusteriAd} {_satis.MusteriSoyad}".Trim();
            
            MusteriAdSoyadTextBox.Text = adSoyad;
            Telefon1TextBox.Text = _satis.Telefon1;
            Telefon2TextBox.Text = _satis.Telefon2;
            TaksitSayisiTextBox.Text = _satis.TaksitSayisi.ToString();
            SatisFiyatiTextBox.Text = _satis.SatisFiyati.ToString("N0", _culture);
            OnOdemeTextBox.Text = _satis.OnOdeme.ToString("N0", _culture);
            AylikOdemeTextBox.Text = _satis.AylikOdeme.ToString("N0", _culture);
            
            // Tarihleri ayarla
            var satisTarihi = _satis.SatisTarihi != default ? _satis.SatisTarihi : _satis.Tarih;
            var taksitTarihi = _satis.TaksitBaslangicTarihi != default ? _satis.TaksitBaslangicTarihi : _satis.Tarih;
            
            SatisTarihiTextBox.Text = satisTarihi.ToString("dd.MM.yyyy");
            TaksitBaslangicTarihiTextBox.Text = taksitTarihi.ToString("dd.MM.yyyy");
        }

        private void LoadPhoneInfo()
        {
            try
            {
                // Eğer satis nesnesinde telefon bilgileri varsa onları kullan
                if (!string.IsNullOrEmpty(_satis.Marka))
                {
                    MarkaTextBox.Text = _satis.Marka;
                    ModelTextBox.Text = _satis.Model;
                    DurumTextBox.Text = _satis.Durum;
                }
                else if (_satis.TelefonId > 0)
                {
                    // Veritabanından telefon bilgilerini çek
                    using (var conn = Database.GetConnection())
                    {
                        conn.Open();
                        string sql = "SELECT Marka, Model, Durum FROM Telefonlar WHERE Id = @TelefonId";
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@TelefonId", _satis.TelefonId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    MarkaTextBox.Text = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                    ModelTextBox.Text = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                    DurumTextBox.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                }
                                else
                                {
                                    MarkaTextBox.Text = "Bilinmiyor";
                                    ModelTextBox.Text = "Bilinmiyor";
                                    DurumTextBox.Text = "Bilinmiyor";
                                }
                            }
                        }
                    }
                }
                else
                {
                    MarkaTextBox.Text = "Bilgi Yok";
                    ModelTextBox.Text = "Bilgi Yok";
                    DurumTextBox.Text = "Bilgi Yok";
                }
            }
            catch (Exception)
            {
                MarkaTextBox.Text = "Hata";
                ModelTextBox.Text = "Hata";
                DurumTextBox.Text = "Hata";
            }
        }

        private void Button_Kaydet_Click(object sender, RoutedEventArgs e)
        {
            string adSoyad = MusteriAdSoyadTextBox.Text.Trim();
            string tel1 = Telefon1TextBox.Text.Trim();
            string? tel2 = string.IsNullOrWhiteSpace(Telefon2TextBox.Text) ? null : Telefon2TextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(adSoyad) || string.IsNullOrWhiteSpace(tel1))
            {
                ThemedMessageBox.Show("Ad soyad ve telefon 1 zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParsePrice(SatisFiyatiTextBox.Text, out double satisFiyati) || satisFiyati < 0)
            {
                ThemedMessageBox.Show("Geçerli bir satış fiyatı girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double onOdeme = 0;
            if (!string.IsNullOrWhiteSpace(OnOdemeTextBox.Text))
            {
                if (!TryParsePrice(OnOdemeTextBox.Text, out onOdeme) || onOdeme < 0)
                {
                    ThemedMessageBox.Show("Geçerli bir ön ödeme tutarı girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (onOdeme >= satisFiyati)
            {
                ThemedMessageBox.Show("Ön ödeme satış fiyatından küçük olmalıdır.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseDate(SatisTarihiTextBox.Text, out DateTime satisTarihi))
            {
                ThemedMessageBox.Show("Geçerli bir satış tarihi girin (GG.AA.YYYY).", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseDate(TaksitBaslangicTarihiTextBox.Text, out DateTime taksitBaslangicTarihi))
            {
                ThemedMessageBox.Show("Geçerli bir taksit başlangıç tarihi girin (GG.AA.YYYY).", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double aylikOdeme;
            if (!TryParsePrice(AylikOdemeTextBox.Text, out aylikOdeme))
            {
                // Kalan tutarı hesapla ve taksite böl
                double kalanTutar = satisFiyati - onOdeme;
                aylikOdeme = _satis.TaksitSayisi > 0 ? Math.Round(kalanTutar / _satis.TaksitSayisi, 2) : 0;
            }

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE TaksitliSatislar 
                               SET MusteriAd = @Ad, MusteriSoyad = @Soyad, MusteriAdSoyad = @AdSoyad, Telefon1 = @Tel1, Telefon2 = @Tel2, 
                                   SatisFiyati = @SatisFiyati, OnOdeme = @OnOdeme, AylikOdeme = @Aylik, SatisTarihi = @SatisTarihi, 
                                   TaksitBaslangicTarihi = @TaksitBaslangic
                               WHERE Id = @Id;";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    // Eski alanları da doldur (geriye uyumluluk için)
                    var parts = adSoyad.Split(' ', 2);
                    var ad = parts.Length > 0 ? parts[0] : "";
                    var soyad = parts.Length > 1 ? parts[1] : "";

                    cmd.Parameters.AddWithValue("@Ad", ad);
                    cmd.Parameters.AddWithValue("@Soyad", soyad);
                    cmd.Parameters.AddWithValue("@AdSoyad", adSoyad);
                    cmd.Parameters.AddWithValue("@Tel1", tel1);
                    cmd.Parameters.AddWithValue("@Tel2", (object?)tel2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SatisFiyati", satisFiyati);
                    cmd.Parameters.AddWithValue("@OnOdeme", onOdeme);
                    cmd.Parameters.AddWithValue("@Aylik", aylikOdeme);
                    cmd.Parameters.AddWithValue("@SatisTarihi", satisTarihi);
                    cmd.Parameters.AddWithValue("@TaksitBaslangic", taksitBaslangicTarihi);
                    cmd.Parameters.AddWithValue("@Id", _satis.Id);
                    cmd.ExecuteNonQuery();
                }
            }

            DialogResult = true;
            Close();
        }

        private void OnOdemeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormattingPrice) return;

            try
            {
                _isFormattingPrice = true;
                if (TryParsePrice(OnOdemeTextBox.Text, out double value))
                {
                    OnOdemeTextBox.Text = value.ToString("N0", _culture);
                    OnOdemeTextBox.CaretIndex = OnOdemeTextBox.Text.Length;
                    
                    // Aylık ödemeyi yeniden hesapla
                    if (SatisFiyatiTextBox != null && AylikOdemeTextBox != null)
                    {
                        if (TryParsePrice(SatisFiyatiTextBox.Text, out double satisFiyati))
                        {
                            double kalanTutar = satisFiyati - value;
                            double aylikOdeme = _satis.TaksitSayisi > 0 ? Math.Round(kalanTutar / _satis.TaksitSayisi, 2) : 0;
                            AylikOdemeTextBox.Text = aylikOdeme.ToString("N0", _culture);
                        }
                    }
                }
            }
            finally
            {
                _isFormattingPrice = false;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private bool TryParsePrice(string input, out double value)
        {
            var normalized = input.Replace(".", "").Replace(",", "");
            return double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private bool TryParseDate(string input, out DateTime date)
        {
            return DateTime.TryParseExact(input, "dd.MM.yyyy", _culture, DateTimeStyles.None, out date) ||
                   DateTime.TryParse(input, _culture, DateTimeStyles.None, out date);
        }

        private void SatisFiyatiTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormattingPrice) return;

            try
            {
                _isFormattingPrice = true;
                if (TryParsePrice(SatisFiyatiTextBox.Text, out double value))
                {
                    SatisFiyatiTextBox.Text = value.ToString("N0", _culture);
                    SatisFiyatiTextBox.CaretIndex = SatisFiyatiTextBox.Text.Length;
                }
            }
            finally
            {
                _isFormattingPrice = false;
            }
        }

        private void AylikOdemeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormattingPrice) return;

            try
            {
                _isFormattingPrice = true;
                if (TryParsePrice(AylikOdemeTextBox.Text, out double value))
                {
                    AylikOdemeTextBox.Text = value.ToString("N0", _culture);
                    AylikOdemeTextBox.CaretIndex = AylikOdemeTextBox.Text.Length;
                }
            }
            finally
            {
                _isFormattingPrice = false;
            }
        }

        private void SatisFiyatiTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (var ch in e.Text)
            {
                if (!char.IsDigit(ch))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private void OnOdemeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (var ch in e.Text)
            {
                if (!char.IsDigit(ch))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private void AylikOdemeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (var ch in e.Text)
            {
                if (!char.IsDigit(ch))
                {
                    e.Handled = true;
                    break;
                }
            }
        }
    }
}

