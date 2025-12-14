using System.Configuration;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace TelefonSatısApp
{
    /// <summary>
    /// Ana uygulama sınıfı - Uygulamanın başlangıç noktası ve genel ayarları
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Uygulama başlatıldığında çalışan metod - Türkçe dil ayarlarını yapar
        /// </summary>
        /// <param name="e">Başlangıç parametreleri</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Uygulama genelinde tarih/sayı biçimlerini Türkçe yapmak için kültürü ayarla
            var culture = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            
            // WPF kontrollerinin de Türkçe dil ayarlarını kullanmasını sağla
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            base.OnStartup(e);
        }
    }

}
