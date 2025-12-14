using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TelefonSatısApp
{
    /// <summary>
    /// PesinSatisiDuzenle.xaml etkileşim mantığı
    /// </summary>
    public partial class PesinSatisiDuzenle : Window
    {
        private PesinSatis _selectedSatis;
        private bool _isFormattingPrice = false;
        private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("tr-TR");

        public PesinSatisiDuzenle(PesinSatis satis)
        {
            InitializeComponent();
            _selectedSatis = satis;

            // Seçilen satışın bilgilerini form elemanlarına aktar
            MarkaTextBox.Text = _selectedSatis.Marka ?? "";
            ModelTextBox.Text = _selectedSatis.Model ?? "";
            DurumTextBox.Text = _selectedSatis.Durum ?? "";
            MusteriAdTextBox.Text = _selectedSatis.MusteriAd;
            MusteriSoyadTextBox.Text = _selectedSatis.MusteriSoyad;
            MusteriTelefonTextBox.Text = _selectedSatis.MusteriTelefon;
            SatisFiyatiTextBox.Text = _selectedSatis.SatisFiyati.ToString("N0", _culture);
            KarTextBox.Text = _selectedSatis.Kar.ToString("N0", _culture);
            TarihTextBox.Text = _selectedSatis.Tarih?.ToString("dd.MM.yyyy") ?? DateTime.Now.ToString("dd.MM.yyyy");
        }

        private void Button_Kaydet_Click(object sender, RoutedEventArgs e)
        {
            // Düzenlenen bilgileri alalım
            _selectedSatis.MusteriAd = MusteriAdTextBox.Text;
            _selectedSatis.MusteriSoyad = MusteriSoyadTextBox.Text;
            _selectedSatis.MusteriTelefon = MusteriTelefonTextBox.Text;
            _selectedSatis.SatisFiyati = TryParsePrice(SatisFiyatiTextBox.Text, out double satisFiyati) ? satisFiyati : 0;
            _selectedSatis.Kar = TryParsePrice(KarTextBox.Text, out double kar) ? kar : 0;
            _selectedSatis.Tarih = TryParseDate(TarihTextBox.Text, out DateTime tarih) ? tarih : DateTime.Now;

            // Veritabanında güncelleme işlemi
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE PesinSatislar SET MusteriAd = @MusteriAd, MusteriSoyad = @MusteriSoyad, MusteriTelefon = @MusteriTelefon, Marka = @Marka, Model = @Model, Durum = @Durum, SatisFiyati = @SatisFiyati, Kar = @Kar, Tarih = @Tarih WHERE MusteriTelefon = @OriginalMusteriTelefon";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    // Orijinal telefon numarasını sakla (WHERE koşulu için)
                    string originalTelefon = _selectedSatis.MusteriTelefon ?? "";
                    
                    cmd.Parameters.AddWithValue("@MusteriAd", _selectedSatis.MusteriAd);
                    cmd.Parameters.AddWithValue("@MusteriSoyad", _selectedSatis.MusteriSoyad);
                    cmd.Parameters.AddWithValue("@MusteriTelefon", MusteriTelefonTextBox.Text);
                    cmd.Parameters.AddWithValue("@Marka", MarkaTextBox.Text);
                    cmd.Parameters.AddWithValue("@Model", ModelTextBox.Text);
                    cmd.Parameters.AddWithValue("@Durum", DurumTextBox.Text);
                    cmd.Parameters.AddWithValue("@SatisFiyati", _selectedSatis.SatisFiyati);
                    cmd.Parameters.AddWithValue("@Kar", _selectedSatis.Kar);
                    cmd.Parameters.AddWithValue("@Tarih", _selectedSatis.Tarih.HasValue ? _selectedSatis.Tarih.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OriginalMusteriTelefon", originalTelefon);

                    cmd.ExecuteNonQuery();
                }
            }

            // Düzenleme işleminden sonra pencereyi kapat
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private bool TryParsePrice(string input, out double value)
        {
            // Nokta/virgül ayracını temizleyerek sadece rakamları parse ediyoruz.
            var normalized = input.Replace(".", "").Replace(",", "");
            return double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private bool TryParseDate(string input, out DateTime date)
        {
            // Türkçe tarih formatını parse et (GG.AA.YYYY)
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

        private void KarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormattingPrice) return;

            try
            {
                _isFormattingPrice = true;
                if (TryParsePrice(KarTextBox.Text, out double value))
                {
                    KarTextBox.Text = value.ToString("N0", _culture);
                    KarTextBox.CaretIndex = KarTextBox.Text.Length;
                }
            }
            finally
            {
                _isFormattingPrice = false;
            }
        }

        private void SatisFiyatiTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

        private void KarTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
    }

}
