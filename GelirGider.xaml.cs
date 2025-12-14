using System;
using System.Data.SQLite;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TelefonSatısApp
{
    public partial class GelirGider : Page
    {
        public GelirGider()
        {
            InitializeComponent();
            InitializeYearComboBox();
            LoadData();
        }

        private void InitializeYearComboBox()
        {
            // Son 5 yılı ekle
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear; year >= currentYear - 4; year--)
            {
                var item = new ComboBoxItem { Content = year.ToString() };
                if (year == currentYear)
                    item.IsSelected = true;
                CbYil.Items.Add(item);
            }
        }

        private void LoadData()
        {
            try
            {
                var (startDate, endDate) = GetDateRange();
                
                // Gelir hesapla (Peşin + Taksitli satışlar)
                double pesinGelir = GetPesinSatisGelir(startDate, endDate);
                double taksitliGelir = GetTaksitliSatisGelir(startDate, endDate);
                double toplamGelir = pesinGelir + taksitliGelir;

                // Gider hesapla (Telefon alış maliyetleri)
                double toplamGider = GetTelefonAlisMaliyeti(startDate, endDate);

                // Net kar
                double netKar = toplamGelir - toplamGider;

                // Satış adedi
                int satisAdedi = GetSatisAdedi(startDate, endDate);

                // UI'yi güncelle
                TxtToplamGelir.Text = $"{toplamGelir:N0} ₺";
                TxtToplamGider.Text = $"{toplamGider:N0} ₺";
                TxtNetKar.Text = $"{netKar:N0} ₺";
                TxtSatisAdedi.Text = satisAdedi.ToString();

                // Net kar rengini ayarla
                if (netKar > 0)
                    TxtNetKar.Parent.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(16, 185, 129))); // Yeşil
                else if (netKar < 0)
                    TxtNetKar.Parent.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(239, 68, 68))); // Kırmızı

                // Detaylı raporu yükle
                LoadDetailedReport(startDate, endDate, pesinGelir, taksitliGelir, toplamGider, netKar, satisAdedi);
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show($"Veri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private (DateTime startDate, DateTime endDate) GetDateRange()
        {
            // Editable ComboBox için Text property kullan
            var selectedDonem = CbDonem.Text ?? (CbDonem.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Bu Ay";
            var yearText = CbYil.Text ?? (CbYil.SelectedItem as ComboBoxItem)?.Content.ToString() ?? DateTime.Now.Year.ToString();
            
            if (!int.TryParse(yearText, out int selectedYear))
                selectedYear = DateTime.Now.Year;

            return selectedDonem switch
            {
                "Bu Ay" => (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), 
                           new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).AddDays(1).AddSeconds(-1)),
                "Bu Yıl" => (new DateTime(selectedYear, 1, 1), 
                             new DateTime(selectedYear, 12, 31, 23, 59, 59)),
                "Tüm Zamanlar" => (DateTime.MinValue, DateTime.MaxValue),
                _ => (DateTime.MinValue, DateTime.MaxValue)
            };
        }

        private double GetPesinSatisGelir(DateTime startDate, DateTime endDate)
        {
            double toplam = 0;
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT COALESCE(SUM(SatisFiyati), 0) FROM PesinSatislar 
                              WHERE Tarih BETWEEN @StartDate AND @EndDate";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                        toplam = Convert.ToDouble(result);
                }
            }
            return toplam;
        }

        private double GetTaksitliSatisGelir(DateTime startDate, DateTime endDate)
        {
            double toplam = 0;
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT COALESCE(SUM(SatisFiyati), 0) FROM TaksitliSatislar 
                              WHERE Tarih BETWEEN @StartDate AND @EndDate";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                        toplam = Convert.ToDouble(result);
                }
            }
            return toplam;
        }

        private double GetTelefonAlisMaliyeti(DateTime startDate, DateTime endDate)
        {
            double toplam = 0;
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                // Telefon alışlarından giderler (direkt alış maliyetleri)
                string alisSql = @"SELECT COALESCE(SUM(AlinanFiyat), 0) FROM TelefonAlislari 
                                  WHERE Tarih BETWEEN @StartDate AND @EndDate";
                using (var cmd = new SQLiteCommand(alisSql, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                        toplam += Convert.ToDouble(result);
                }
            }
            return toplam;
        }

        private int GetSatisAdedi(DateTime startDate, DateTime endDate)
        {
            int toplam = 0;
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT 
                        (SELECT COUNT(*) FROM PesinSatislar WHERE Tarih BETWEEN @StartDate AND @EndDate) +
                        (SELECT COUNT(*) FROM TaksitliSatislar WHERE Tarih BETWEEN @StartDate AND @EndDate)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                        toplam = Convert.ToInt32(result);
                }
            }
            return toplam;
        }

        private void LoadDetailedReport(DateTime startDate, DateTime endDate, double pesinGelir, double taksitliGelir, double toplamGider, double netKar, int satisAdedi)
        {
            DetayliRaporPanel.Children.Clear();

            // Gelirler Başlığı
            var gelirBaslik = CreateReportSection("📈 GELİRLER", "", "#1F2937");
            DetayliRaporPanel.Children.Add(gelirBaslik);

            // Peşin Satışlar
            var pesinPanel = CreateReportSection("💰 Peşin Satışlar", $"{pesinGelir:N0} ₺", "#10B981");
            DetayliRaporPanel.Children.Add(pesinPanel);

            // Taksitli Satışlar
            var taksitliPanel = CreateReportSection("📅 Taksitli Satışlar", $"{taksitliGelir:N0} ₺", "#3B82F6");
            DetayliRaporPanel.Children.Add(taksitliPanel);

            // Giderler Başlığı
            var giderBaslik = CreateReportSection("📉 GİDERLER", "", "#1F2937");
            DetayliRaporPanel.Children.Add(giderBaslik);

            // Telefon Alış Maliyetleri
            var giderPanel = CreateReportSection("📱 Telefon Alışları", $"{toplamGider:N0} ₺", "#EF4444");
            DetayliRaporPanel.Children.Add(giderPanel);

            // Analiz Başlığı
            var analizBaslik = CreateReportSection("📊 ANALİZ", "", "#1F2937");
            DetayliRaporPanel.Children.Add(analizBaslik);

            // Kar Marjı
            double toplamGelir = pesinGelir + taksitliGelir;
            double karMarji = toplamGelir > 0 ? (netKar / toplamGelir) * 100 : 0;
            var marjPanel = CreateReportSection("📊 Kar Marjı", $"%{karMarji:N1}", "#8B5CF6");
            DetayliRaporPanel.Children.Add(marjPanel);

            // Ortalama Satış Fiyatı
            double ortalamaSatis = satisAdedi > 0 ? toplamGelir / satisAdedi : 0;
            var ortalamaPanel = CreateReportSection("💵 Ortalama Satış Fiyatı", $"{ortalamaSatis:N0} ₺", "#F59E0B");
            DetayliRaporPanel.Children.Add(ortalamaPanel);

            // Telefon Alış Adedi
            int alisAdedi = GetTelefonAlisAdedi(startDate, endDate);
            var alisAdediPanel = CreateReportSection("🛒 Telefon Alış Adedi", alisAdedi.ToString(), "#6B7280");
            DetayliRaporPanel.Children.Add(alisAdediPanel);
        }

        private Border CreateReportSection(string title, string value, string colorHex)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Eğer value boşsa, bu bir başlık
            if (string.IsNullOrEmpty(value))
            {
                var titleBlock = new TextBlock
                {
                    Text = title,
                    Foreground = Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                border.Child = titleBlock;
            }
            else
            {
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var titleBlock = new TextBlock
                {
                    Text = title,
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(titleBlock, 0);

                var valueBlock = new TextBlock
                {
                    Text = value,
                    Foreground = Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(valueBlock, 1);

                grid.Children.Add(titleBlock);
                grid.Children.Add(valueBlock);
                border.Child = grid;
            }

            return border;
        }

        private void CbDonem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                LoadData();
        }

        private void CbYil_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                LoadData();
        }

        private void CbDonem_DropDownClosed(object sender, EventArgs e)
        {
            if (IsLoaded)
                LoadData();
        }

        private void CbDonem_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                LoadData();
        }

        private void CbYil_DropDownClosed(object sender, EventArgs e)
        {
            if (IsLoaded)
                LoadData();
        }

        private void CbYil_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                LoadData();
        }

        private void ComboBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && IsLoaded)
            {
                LoadData();
            }
        }

        private int GetTelefonAlisAdedi(DateTime startDate, DateTime endDate)
        {
            int toplam = 0;
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM TelefonAlislari 
                              WHERE Tarih BETWEEN @StartDate AND @EndDate";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                        toplam = Convert.ToInt32(result);
                }
            }
            return toplam;
        }

        private void YenileButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void Button_ExcelExport(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    Title = "CSV Dosyası Kaydet",
                    FileName = $"GelirGiderRaporu_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var (startDate, endDate) = GetDateRange();
                    var pesinGelir = GetPesinSatisGelir(startDate, endDate);
                    var taksitliGelir = GetTaksitliSatisGelir(startDate, endDate);
                    var toplamGider = GetTelefonAlisMaliyeti(startDate, endDate);
                    var netKar = (pesinGelir + taksitliGelir) - toplamGider;
                    var satisAdedi = GetSatisAdedi(startDate, endDate);
                    var alisAdedi = GetTelefonAlisAdedi(startDate, endDate);

                    var csv = "Rapor Türü,Değer\n";
                    csv += $"\"Dönem\",\"{CbDonem.Text}\"\n";
                    csv += $"\"Yıl\",\"{CbYil.Text}\"\n";
                    csv += $"\"Peşin Satış Geliri\",\"{pesinGelir:N0} ₺\"\n";
                    csv += $"\"Taksitli Satış Geliri\",\"{taksitliGelir:N0} ₺\"\n";
                    csv += $"\"Toplam Gelir\",\"{(pesinGelir + taksitliGelir):N0} ₺\"\n";
                    csv += $"\"Toplam Gider\",\"{toplamGider:N0} ₺\"\n";
                    csv += $"\"Net Kar\",\"{netKar:N0} ₺\"\n";
                    csv += $"\"Satış Adedi\",\"{satisAdedi}\"\n";
                    csv += $"\"Alış Adedi\",\"{alisAdedi}\"\n";

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csv, System.Text.Encoding.UTF8);
                    
                    ThemedMessageBox.Show($"Gelir gider raporu başarıyla dışa aktarıldı!\nDosya: {saveFileDialog.FileName}", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show($"Dışa aktarım sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}