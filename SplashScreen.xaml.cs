using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TelefonSatısApp
{
    /// <summary>
    /// Başlangıç ekranı sınıfı - Uygulama açılırken gösterilen yükleme ekranı
    /// </summary>
    public partial class SplashScreen : Window
    {
        // Yükleme mesajları için zamanlayıcı
        private DispatcherTimer? _timer;
        
        // Hangi yükleme adımında olduğumuzu takip eder
        private int _loadingStep = 0;
        
        // Yükleme sırasında gösterilecek mesajlar
        private string[] _loadingMessages = {
            "Başlatılıyor...",
            "Yükleniyor...",
            "Hazır!"
        };

        /// <summary>
        /// Başlangıç ekranı yapıcı metodu - Ekranı başlatır ve yükleme sürecini başlatır
        /// </summary>
        public SplashScreen()
        {
            InitializeComponent();
            CheckLogoImage(); // Logo resmini kontrol et
            StartLoadingProcess(); // Yükleme sürecini başlat
        }

        /// <summary>
        /// Logo resminin varlığını kontrol eder, yoksa gizler
        /// </summary>
        private void CheckLogoImage()
        {
            try
            {
                // Logo resmi yoksa veya yüklenemezse, text logo göster
                var uri = new Uri("pack://application:,,,/Resources/logo.png");
                var resourceStream = Application.GetResourceStream(uri);
                if (resourceStream == null)
                {
                    LogoImage.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                // Hata durumunda logo resmini gizle, text logo göster
                LogoImage.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Yükleme sürecini başlatır - Zamanlayıcıyı ayarlar ve çalıştırır
        /// </summary>
        private void StartLoadingProcess()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500); // Her 500ms'de bir güncelle
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        /// <summary>
        /// Zamanlayıcı her tetiklendiğinde çalışır - Yükleme mesajlarını günceller
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_loadingStep < _loadingMessages.Length)
            {
                LoadingText.Text = _loadingMessages[_loadingStep];
                _loadingStep++;
            }
            else
            {
                _timer?.Stop(); // Zamanlayıcıyı durdur
                CompleteLoading(); // Yükleme tamamlandı
            }
        }

        /// <summary>
        /// Yükleme tamamlandığında çalışır - Ana pencereyi açar ve splash screen'i kapatır
        /// </summary>
        private async void CompleteLoading()
        {
            // Kısa bir bekleme süresi (görsel efekt için)
            await Task.Delay(200);

            // Ana pencereyi oluştur ve göster
            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            // Splash screen'i kapat
            this.Close();
        }

        /// <summary>
        /// Pencere başlatıldığında çalışır - Pencereyi ekranın ortasına yerleştirir
        /// </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Pencereyi ekranın ortasına yerleştir
            var workingArea = SystemParameters.WorkArea;
            this.Left = (workingArea.Width - this.Width) / 2 + workingArea.Left;
            this.Top = (workingArea.Height - this.Height) / 2 + workingArea.Top;
        }
    }
}