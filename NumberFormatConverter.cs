using System;
using System.Globalization;
using System.Windows.Data;

namespace TelefonSatısApp
{
    /// <summary>
    /// Sayı formatı dönüştürücü sınıfı - XAML'de sayıları binlik ayırıcıyla göstermek için kullanılır
    /// </summary>
    public class NumberFormatConverter : IValueConverter
    {
        /// <summary>
        /// Sayıyı binlik ayırıcılı metne dönüştürür (örn: 1000 -> "1.000")
        /// </summary>
        /// <param name="value">Dönüştürülecek sayı değeri</param>
        /// <param name="targetType">Hedef tip</param>
        /// <param name="parameter">Parametre</param>
        /// <param name="culture">Kültür bilgisi</param>
        /// <returns>Formatlanmış metin</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double || value is int)
            {
                return string.Format("{0:N0}", value); // Binlik ayırıcı ekler
            }
            return value;
        }

        /// <summary>
        /// Formatlanmış metni tekrar sayıya dönüştürür
        /// </summary>
        /// <param name="value">Dönüştürülecek metin</param>
        /// <param name="targetType">Hedef tip</param>
        /// <param name="parameter">Parametre</param>
        /// <param name="culture">Kültür bilgisi</param>
        /// <returns>Sayı değeri</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                strValue = strValue.Replace(".", "").Replace(",", ".");
                if (double.TryParse(strValue, out double result))
                {
                    return result;
                }
            }
            return value;
        }
    }
}
