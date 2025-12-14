using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TelefonSatısApp
{
    /// <summary>
    /// Ödeme durumu renk dönüştürücü sınıfı - Taksit ödemelerinin rengini belirler
    /// </summary>
    public class OdemeToBrushConverter : IValueConverter
    {
        // Ödendi durumu için yeşil renk
        private static readonly SolidColorBrush Yesil = new SolidColorBrush(Color.FromRgb(34, 197, 94));   // #22C55E
        
        // Ödenmedi durumu için kırmızı renk
        private static readonly SolidColorBrush Kirmizi = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // #EF4444

        /// <summary>
        /// Ödeme durumunu renge dönüştürür - True ise yeşil, False ise kırmızı
        /// </summary>
        /// <param name="value">Ödeme durumu (bool)</param>
        /// <param name="targetType">Hedef tip</param>
        /// <param name="parameter">Parametre</param>
        /// <param name="culture">Kültür bilgisi</param>
        /// <returns>Renk fırçası</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool odendi = value is bool b && b;
            return odendi ? Yesil : Kirmizi;
        }

        /// <summary>
        /// Geri dönüştürme desteklenmiyor
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

