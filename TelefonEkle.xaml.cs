using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace TelefonSatısApp
{
    public partial class TelefonEkle : Window
    {
        private bool _isFormattingPrice = false;

        private readonly Dictionary<string, List<string>> _modelMap = new()
{
    { "Samsung", new List<string>{
        "Galaxy S23", "Galaxy S23+", "Galaxy S23 Ultra", "Galaxy S22", "Galaxy S22+", "Galaxy S22 Ultra",
        "Galaxy Z Fold5", "Galaxy Z Flip5", "Galaxy Z Fold4", "Galaxy Z Flip4",
        "Galaxy A54", "Galaxy A53", "Galaxy A52", "Galaxy A51", "Galaxy A32",
        "Galaxy M54", "Galaxy M53", "Galaxy M52", "Galaxy M51", "Galaxy M32",
        "Galaxy A34", "Galaxy A33", "Galaxy A73", "Galaxy F54", "Galaxy F53",
        "Galaxy F42", "Galaxy M14", "Galaxy M13", "Galaxy M12", "Galaxy M11",
        "Galaxy XCover 6", "Galaxy A04", "Galaxy A03", "Galaxy A02",
        "Galaxy S21", "Galaxy S21+", "Galaxy S21 Ultra", "Galaxy S20", "Galaxy S20+", "Galaxy S20 Ultra",
        "Galaxy Note 20", "Galaxy Note 20 Ultra", "Galaxy Note 10", "Galaxy Note 10+", "Galaxy Note 9",
        "Galaxy A12", "Galaxy A11", "Galaxy A10", "Galaxy J6", "Galaxy J7", "Galaxy J8"
    }},
    { "Apple", new List<string>{
        "iPhone 17 Pro Max", "iPhone 17 Pro", "iPhone 17", "iPhone 17 Plus",
        "iPhone 16 Pro Max", "iPhone 16 Pro", "iPhone 16", "iPhone 16 Plus",
        "iPhone 15 Pro Max", "iPhone 15 Pro", "iPhone 15", "iPhone 15 Plus",
        "iPhone 14 Pro", "iPhone 14", "iPhone 14 Plus", "iPhone 14 Pro Max",
        "iPhone 13 Pro", "iPhone 13", "iPhone 13 Mini", "iPhone 13 Pro Max",
        "iPhone 12 Pro", "iPhone 12", "iPhone 12 Mini", "iPhone 12 Pro Max",
        "iPhone 11 Pro", "iPhone 11", "iPhone 11 Pro Max",
        "iPhone XS", "iPhone XS Max", "iPhone XR",
        "iPhone X", "iPhone 8", "iPhone 8 Plus", "iPhone 7", "iPhone 7 Plus",
        "iPhone SE (3. nesil)", "iPhone SE (2. nesil)", "iPhone SE (1. nesil)",
        "iPhone 6S", "iPhone 6S Plus", "iPhone 6", "iPhone 6 Plus",
        "iPhone 5S", "iPhone 5C", "iPhone 5"
    }},
    { "Xiaomi", new List<string>{
        "Xiaomi 14", "Xiaomi 14 Pro", "Xiaomi 13T", "Xiaomi 13T Pro", "Xiaomi 13", "Xiaomi 13 Pro",
        "Xiaomi 12T", "Xiaomi 12T Pro", "Xiaomi 12", "Xiaomi 12 Pro", "Xiaomi 12X",
        "Xiaomi 11T", "Xiaomi 11T Pro", "Xiaomi 11", "Xiaomi 11 Pro",
        "Redmi Note 13 Pro", "Redmi Note 13", "Redmi Note 12 Pro", "Redmi Note 12", "Redmi Note 12 5G",
        "Redmi 12", "Redmi 12 Prime", "Redmi 11", "Redmi 11 Prime",
        "Redmi K50", "Redmi K50 Pro", "Redmi K40", "Redmi K40 Pro",
        "Poco F5", "Poco F5 Pro", "Poco X5", "Poco X5 Pro", "Poco X4 Pro",
        "Poco M5", "Poco M4 Pro", "Poco M3"
    }},
    { "Huawei", new List<string>{
        "P60 Pro", "P60", "Mate 50 Pro", "Mate 50", "Mate 50E", "Nova 12",
        "Nova 11", "Nova 11 Pro", "P50 Pro", "P50", "P50E", "Mate X3",
        "Mate 40 Pro", "Mate 40", "Mate 40E", "P40 Pro", "P40", "P40 Lite",
        "P30 Pro", "P30", "P30 Lite", "Y70", "Y60", "Y50", "Honor 50",
        "Honor 50 Pro", "Honor Magic 4", "Honor Magic 4 Pro", "Honor 60", "Honor 60 Pro"
    }},
    { "Oppo", new List<string>{
        "Reno 11", "Reno 10", "Reno 9", "Find X6 Pro", "Find X5 Pro", "Find X5",
        "Find X4 Pro", "Find X4", "Oppo A78", "Oppo A77", "Oppo A76",
        "Oppo A54", "Oppo A53", "Oppo A52", "Oppo F21 Pro", "Oppo F19 Pro",
        "Oppo F17 Pro", "Oppo F15", "Oppo Reno 8 Pro", "Oppo Reno 8", "Oppo Reno 7 Pro"
    }},
    { "Realme", new List<string>{
        "GT 6", "GT Neo 6", "GT 5", "GT 5 Pro", "Realme 11 Pro", "Realme 11",
        "Realme 10 Pro", "Realme 10 Pro+", "Realme 9i", "Realme 9",
        "Realme X3 SuperZoom", "Realme X2 Pro", "Realme X50 Pro", "Realme X50",
        "Realme Narzo 50A", "Realme Narzo 50", "Realme Narzo 30 Pro"
    }},
            { "Reeder" , new List<string>{
            "P13 Blue", "P13 Blue Max"} }

         };


        public TelefonEkle()
        {
            InitializeComponent();

            for (int i = 0; i <= 24; i++)
            {
                CbGarantiAy.Items.Add(i);
            }

            CbGarantiAy.SelectedIndex = 0;

            // Çıkış yılı listesini doldur (2010'dan günümüze + gelecek 2 yıl)
            int currentYear = DateTime.Now.Year;
            for (int year = 2010; year <= currentYear + 2; year++)
            {
                CbCikisYili.Items.Add(year);
            }
            CbCikisYili.SelectedItem = currentYear; // Varsayılan olarak bu yıl
            // Marka listesini yükle
            foreach (var brand in _modelMap.Keys)
            {
                CbMarka.Items.Add(brand);
            }
            CbMarka.SelectedIndex = 0;
            UpdateModelsForBrand("Samsung");
            CbDurum.SelectedIndex = 0;
        }

        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();

                    string sql = @"INSERT INTO Telefonlar 
                        (Imei, Marka, Model, Renk, GarantiAy, CikisYili, AlinanFiyat, Durum)
                        VALUES (@Imei, @Marka, @Model, @Renk, @GarantiAy, @CikisYili, @AlinanFiyat, @Durum)";

                    long telefonId;
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Imei", TxtImei.Text);
                        var marka = CbMarka.Text ?? "";
                        cmd.Parameters.AddWithValue("@Marka", marka);
                        var model = CbModel.Text ?? "";
                        cmd.Parameters.AddWithValue("@Model", model);
                        string renk = (CbRenk.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                        cmd.Parameters.AddWithValue("@Renk", renk);
                        cmd.Parameters.AddWithValue("@GarantiAy", CbGarantiAy.SelectedItem ?? 0);
                        cmd.Parameters.AddWithValue("@CikisYili", int.TryParse(CbCikisYili.Text, out var cy) ? cy : 0);
                        double alinanFiyat = double.TryParse(TxtAlinanFiyat.Text.Replace(".", "").Replace(",", ""), out var a) ? a : 0;
                        cmd.Parameters.AddWithValue("@AlinanFiyat", alinanFiyat);

                        string durumSecim = (CbDurum.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                        cmd.Parameters.AddWithValue("@Durum", NormalizeDurum(durumSecim));

                        cmd.ExecuteNonQuery();
                        telefonId = conn.LastInsertRowId;
                    }

                    // Telefon alışını gider olarak kaydet
                    string giderSql = @"INSERT INTO TelefonAlislari 
                        (TelefonId, Imei, Model, Marka, Renk, AlinanFiyat, Tarih)
                        VALUES (@TelefonId, @Imei, @Model, @Marka, @Renk, @AlinanFiyat, @Tarih)";

                    using (var giderCmd = new SQLiteCommand(giderSql, conn))
                    {
                        giderCmd.Parameters.AddWithValue("@TelefonId", telefonId);
                        giderCmd.Parameters.AddWithValue("@Imei", TxtImei.Text);
                        var model = CbModel.Text ?? "";
                        giderCmd.Parameters.AddWithValue("@Model", model);
                        var marka = CbMarka.Text ?? "";
                        giderCmd.Parameters.AddWithValue("@Marka", marka);
                        string renk = (CbRenk.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                        giderCmd.Parameters.AddWithValue("@Renk", renk);
                        double alinanFiyat = double.TryParse(TxtAlinanFiyat.Text.Replace(".", "").Replace(",", ""), out var a) ? a : 0;
                        giderCmd.Parameters.AddWithValue("@AlinanFiyat", alinanFiyat);
                        giderCmd.Parameters.AddWithValue("@Tarih", DateTime.Now);

                        giderCmd.ExecuteNonQuery();
                    }
                }

                var successDialog = new TemaliMesajPenceresi("Başarılı", "Telefon başarıyla eklendi!");
                successDialog.Owner = this;
                successDialog.ShowDialog();

                this.DialogResult = true;   // Ana sayfa yenilenecek
                this.Close();
            }
            catch (Exception ex)
            {
                var errorDialog = new TemaliMesajPenceresi("Hata", "Hata: " + ex.Message);
                errorDialog.Owner = this;
                errorDialog.ShowDialog();
            }
        }

        private static string NormalizeDurum(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Sıfır";

            var txt = raw.Trim().ToLowerInvariant();
            if (txt.Contains("ikinci") || txt.Contains("2"))
                return "İkinci El";

            return "Sıfır";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CbMarka_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbMarka.SelectedItem is string selectedBrand)
            {
                UpdateModelsForBrand(selectedBrand);
            }
        }

        private void TxtAlinanFiyat_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormattingPrice) return;
            try
            {
                _isFormattingPrice = true;
                string raw = TxtAlinanFiyat.Text.Replace(".", "").Replace(",", "");
                if (double.TryParse(raw, out var value))
                {
                    TxtAlinanFiyat.Text = value.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
                    TxtAlinanFiyat.CaretIndex = TxtAlinanFiyat.Text.Length;
                }
            }
            finally
            {
                _isFormattingPrice = false;
            }
        }

        private void UpdateModelsForBrand(string brand)
        {
            CbModel.Items.Clear();
            if (_modelMap.TryGetValue(brand, out var models))
            {
                foreach (var m in models)
                {
                    CbModel.Items.Add(m);
                }
                if (CbModel.Items.Count > 0)
                {
                    CbModel.SelectedIndex = 0;
                }
            }
            CbModel.IsEnabled = CbModel.Items.Count > 0;
        }
    }
}
