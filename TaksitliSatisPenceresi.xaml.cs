using System;
using System.Globalization;
using System.Windows;

namespace TelefonSatısApp
{
    public partial class TaksitliSatisPenceresi : Window
    {
        private readonly Telefon _telefon;
        private int _taksitSayisi = 1;

        public TaksitliSatisPenceresi(Telefon telefon)
        {
            InitializeComponent();
            _telefon = telefon;
            TxtTelefonBilgi.Text = $" | {telefon.Marka} {telefon.Model} (IMEI: {telefon.Imei})";
            CbTaksitSayisi.SelectedIndex = 0;
            
            // Tarihleri bugün olarak ayarla
            DpSatisTarihi.SelectedDate = DateTime.Today;
            DpTaksitBaslangic.SelectedDate = DateTime.Today;
            
            UpdateAylikOdeme();
        }

        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            string adSoyad = TxtMusteriAdSoyad.Text.Trim();
            string tel1 = TxtTelefon1.Text.Trim();
            string? tel2 = string.IsNullOrWhiteSpace(TxtTelefon2.Text) ? null : TxtTelefon2.Text.Trim();

            if (string.IsNullOrWhiteSpace(adSoyad) || string.IsNullOrWhiteSpace(tel1))
            {
                ThemedMessageBox.Show("Ad soyad ve telefon 1 zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string cleanSatisFiyatiText = TxtSatisFiyati.Text.Trim().Replace(".", "").Replace(",", ".");
            if (!double.TryParse(cleanSatisFiyatiText, NumberStyles.Number, CultureInfo.InvariantCulture, out double satisFiyati) || satisFiyati <= 0)
            {
                ThemedMessageBox.Show("Geçerli bir satış fiyatı girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double onOdeme = 0;
            if (!string.IsNullOrWhiteSpace(TxtOnOdeme.Text))
            {
                string cleanOnOdemeText = TxtOnOdeme.Text.Trim().Replace(".", "").Replace(",", ".");
                if (!double.TryParse(cleanOnOdemeText, NumberStyles.Number, CultureInfo.InvariantCulture, out onOdeme) || onOdeme < 0)
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

            if (!DpSatisTarihi.SelectedDate.HasValue)
            {
                ThemedMessageBox.Show("Satış tarihi seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DpTaksitBaslangic.SelectedDate.HasValue)
            {
                ThemedMessageBox.Show("Taksit başlangıç tarihi seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime satisTarihi = DpSatisTarihi.SelectedDate.Value;
            DateTime taksitBaslangic = DpTaksitBaslangic.SelectedDate.Value;
            
            // Kalan tutarı hesapla (Satış fiyatı - Ön ödeme)
            double kalanTutar = satisFiyati - onOdeme;
            double aylikOdeme = Math.Round(kalanTutar / _taksitSayisi, 2);

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string insertSql = @"INSERT INTO TaksitliSatislar (TelefonId, MusteriAd, MusteriSoyad, MusteriAdSoyad, Telefon1, Telefon2, TaksitSayisi, SatisFiyati, OnOdeme, AylikOdeme, SatisTarihi, TaksitBaslangicTarihi, AlinanFiyat, Tarih)
                                     VALUES (@TelefonId, @Ad, @Soyad, @AdSoyad, @Tel1, @Tel2, @TaksitSayisi, @SatisFiyati, @OnOdeme, @AylikOdeme, @SatisTarihi, @TaksitBaslangic, @AlinanFiyat, @Tarih);";
                long satisId;
                using (var cmd = new System.Data.SQLite.SQLiteCommand(insertSql, conn))
                {
                    // Eski alanları da doldur (geriye uyumluluk için)
                    var parts = adSoyad.Split(' ', 2);
                    var ad = parts.Length > 0 ? parts[0] : "";
                    var soyad = parts.Length > 1 ? parts[1] : "";

                    cmd.Parameters.AddWithValue("@TelefonId", _telefon.Id);
                    cmd.Parameters.AddWithValue("@Ad", ad);
                    cmd.Parameters.AddWithValue("@Soyad", soyad);
                    cmd.Parameters.AddWithValue("@AdSoyad", adSoyad);
                    cmd.Parameters.AddWithValue("@Tel1", tel1);
                    cmd.Parameters.AddWithValue("@Tel2", (object?)tel2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaksitSayisi", _taksitSayisi);
                    cmd.Parameters.AddWithValue("@SatisFiyati", satisFiyati);
                    cmd.Parameters.AddWithValue("@OnOdeme", onOdeme);
                    cmd.Parameters.AddWithValue("@AylikOdeme", aylikOdeme);
                    cmd.Parameters.AddWithValue("@SatisTarihi", satisTarihi);
                    cmd.Parameters.AddWithValue("@TaksitBaslangic", taksitBaslangic);
                    cmd.Parameters.AddWithValue("@AlinanFiyat", _telefon.AlinanFiyat);
                    cmd.Parameters.AddWithValue("@Tarih", DateTime.Now);
                    cmd.ExecuteNonQuery();
                    satisId = conn.LastInsertRowId;
                }

                // Her taksit için vade kaydı oluştur - taksit başlangıç tarihinden itibaren
                for (int i = 1; i <= _taksitSayisi; i++)
                {
                    DateTime vade = taksitBaslangic.AddMonths(i - 1);
                    string odemeSql = @"INSERT INTO TaksitOdemeleri (TaksitliSatisId, TaksitNo, VadeTarihi, Odendi) 
                                        VALUES (@SatisId, @No, @Vade, 0);";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(odemeSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SatisId", satisId);
                        cmd.Parameters.AddWithValue("@No", i);
                        cmd.Parameters.AddWithValue("@Vade", vade);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Satılan telefonu envanterden çıkar
                using (var deleteCmd = new System.Data.SQLite.SQLiteCommand("DELETE FROM Telefonlar WHERE Id = @Id;", conn))
                {
                    deleteCmd.Parameters.AddWithValue("@Id", _telefon.Id);
                    deleteCmd.ExecuteNonQuery();
                }
            }

            ThemedMessageBox.Show("Taksitli satış kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void UpdateAylikOdeme()
        {
            if (TxtAylikOdeme == null || TxtSatisFiyati == null || TxtOnOdeme == null || TxtOnOdemeGoster == null || TxtKalanTutar == null)
                return;

            var satisFiyatiText = TxtSatisFiyati.Text.Trim();
            var onOdemeText = TxtOnOdeme.Text.Trim();
            
            // Satış fiyatını parse et (binlik ayırıcıları temizle)
            string cleanSatisFiyati = satisFiyatiText.Replace(".", "").Replace(",", ".");
            if (double.TryParse(cleanSatisFiyati, NumberStyles.Number, CultureInfo.InvariantCulture, out double satisFiyati) && satisFiyati > 0)
            {
                // Ön ödemeyi parse et (binlik ayırıcıları temizle)
                double onOdeme = 0;
                if (!string.IsNullOrWhiteSpace(onOdemeText))
                {
                    string cleanOnOdeme = onOdemeText.Replace(".", "").Replace(",", ".");
                    double.TryParse(cleanOnOdeme, NumberStyles.Number, CultureInfo.InvariantCulture, out onOdeme);
                }

                // Ön ödeme satış fiyatından büyük olamaz
                if (onOdeme > satisFiyati)
                {
                    onOdeme = satisFiyati;
                    TxtOnOdeme.Text = onOdeme.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
                }

                // Kalan tutarı hesapla
                double kalanTutar = satisFiyati - onOdeme;
                double aylik = Math.Round(kalanTutar / _taksitSayisi, 2);

                // UI'yi güncelle
                TxtOnOdemeGoster.Text = $"{onOdeme:N0} ₺";
                TxtKalanTutar.Text = $"{kalanTutar:N0} ₺";
                TxtAylikOdeme.Text = $"{aylik:N0} ₺";
            }
            else
            {
                TxtOnOdemeGoster.Text = "0 ₺";
                TxtKalanTutar.Text = "0 ₺";
                TxtAylikOdeme.Text = "0 ₺";
            }
        }

        private void TxtSatisFiyati_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            // Cursor pozisyonunu kaydet
            int cursorPosition = textBox.SelectionStart;
            string originalText = textBox.Text;

            // Sadece rakamları al
            string numbersOnly = System.Text.RegularExpressions.Regex.Replace(originalText, @"[^\d]", "");

            if (!string.IsNullOrEmpty(numbersOnly))
            {
                // Sayıyı parse et ve binlik ayırıcı ile formatla
                if (long.TryParse(numbersOnly, out long number))
                {
                    string formattedText = number.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
                    
                    // Eğer metin değiştiyse güncelle
                    if (textBox.Text != formattedText)
                    {
                        textBox.Text = formattedText;
                        
                        // Cursor pozisyonunu ayarla
                        int newCursorPosition = Math.Min(cursorPosition + (formattedText.Length - originalText.Length), formattedText.Length);
                        textBox.SelectionStart = Math.Max(0, newCursorPosition);
                    }
                }
            }

            UpdateAylikOdeme();
        }

        private void TxtOnOdeme_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            // Cursor pozisyonunu kaydet
            int cursorPosition = textBox.SelectionStart;
            string originalText = textBox.Text;

            // Sadece rakamları al
            string numbersOnly = System.Text.RegularExpressions.Regex.Replace(originalText, @"[^\d]", "");

            if (!string.IsNullOrEmpty(numbersOnly))
            {
                // Sayıyı parse et ve binlik ayırıcı ile formatla
                if (long.TryParse(numbersOnly, out long number))
                {
                    string formattedText = number.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
                    
                    // Eğer metin değiştiyse güncelle
                    if (textBox.Text != formattedText)
                    {
                        textBox.Text = formattedText;
                        
                        // Cursor pozisyonunu ayarla
                        int newCursorPosition = Math.Min(cursorPosition + (formattedText.Length - originalText.Length), formattedText.Length);
                        textBox.SelectionStart = Math.Max(0, newCursorPosition);
                    }
                }
            }
            else if (string.IsNullOrEmpty(originalText))
            {
                // Boş ise 0 yap
                textBox.Text = "0";
                textBox.SelectionStart = 1;
            }

            UpdateAylikOdeme();
        }

        private void CbTaksitSayisi_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CbTaksitSayisi.SelectedItem is System.Windows.Controls.ComboBoxItem item && item.Content != null && int.TryParse(item.Content.ToString(), out int value))
            {
                _taksitSayisi = value;
                UpdateAylikOdeme();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

