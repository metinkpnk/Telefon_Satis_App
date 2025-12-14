using System;
using System.Data.SQLite;
using System.Text;
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
    /// Ana pencere sınıfı - Uygulamanın ana arayüzü ve navigasyon kontrolü
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Ana pencere yapıcı metodu - Pencereyi başlatır ve veritabanını hazırlar
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Database.Initialize(); // Veritabanını başlat
            this.WindowState = WindowState.Maximized; // Pencereyi tam ekran aç
            MainContent.Navigate(new AnaSyafa()); // Ana sayfayı yükle
            UpdateNotificationBadge(); // Bildirim sayısını güncelle
        }

        /// <summary>
        /// Ana Sayfa butonuna tıklandığında çalışır
        /// </summary>
        private void AnaSayfaButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Navigate(new AnaSyafa());
            UpdateNotificationBadge();
        }

        /// <summary>
        /// Peşin Satışlar butonuna tıklandığında çalışır
        /// </summary>
        private void PesinSatıslarButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Navigate(new PesinSatislar()); // Peşin Satışlar sayfasına yönlendirme
            UpdateNotificationBadge();
        }

        /// <summary>
        /// Taksitli Satışlar butonuna tıklandığında çalışır
        /// </summary>
        private void TaksitliSatislarButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Navigate(new TaksitliSatislar());
            UpdateNotificationBadge();
        }

        /// <summary>
        /// Taksit Takip butonuna tıklandığında çalışır
        /// </summary>
        private void TaksitTakipButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Navigate(new TaksitTakip());
            UpdateNotificationBadge();
        }

        /// <summary>
        /// Gelir Gider butonuna tıklandığında çalışır
        /// </summary>
        private void GelirGiderButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Navigate(new GelirGider());
            UpdateNotificationBadge();
        }

        /// <summary>
        /// Bildirimler butonuna tıklandığında çalışır
        /// </summary>
        private void BildirimlerButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Navigate(new Bildirimler());
            UpdateNotificationBadge();
        }

        /// <summary>
        /// Bildirim rozetini günceller - Acil ödemelerin sayısını gösterir
        /// </summary>
        private void UpdateNotificationBadge()
        {
            try
            {
                int urgentCount = GetUrgentNotificationCount();
                
                if (urgentCount > 0)
                {
                    NotificationBadge.Visibility = Visibility.Visible;
                    NotificationCount.Text = urgentCount > 99 ? "99+" : urgentCount.ToString();
                }
                else
                {
                    NotificationBadge.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                // Hata durumunda badge'i gizle
                NotificationBadge.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Acil bildirim sayısını hesaplar - Bugün ve geciken ödemeleri sayar
        /// </summary>
        /// <returns>Acil ödeme sayısı</returns>
        private int GetUrgentNotificationCount()
        {
            var bugun = DateTime.Today;
            int count = 0;

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                // Bugünkü ve geciken ödemeleri say
                string sql = @"
                    SELECT COUNT(*) 
                    FROM TaksitliSatislar s
                    INNER JOIN TaksitOdemeleri t ON t.TaksitliSatisId = s.Id
                    WHERE t.Odendi = 0 AND t.VadeTarihi <= @Bugun";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Bugun", bugun.AddDays(1).AddSeconds(-1));
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                        count = Convert.ToInt32(result);
                }
            }

            return count;
        }

        /// <summary>
        /// Pencereyi küçültme butonu
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Pencereyi büyütme/küçültme butonu
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        /// <summary>
        /// Uygulamayı kapatma butonu
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}