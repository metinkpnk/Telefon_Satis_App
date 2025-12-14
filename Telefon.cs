using System;
using System.Globalization;

namespace TelefonSatısApp
{
    /// <summary>
    /// Telefon model sınıfı - Stokta bulunan telefonların bilgilerini tutar
    /// </summary>
    public class Telefon
    {
        /// <summary>Telefon benzersiz kimlik numarası</summary>
        public int Id { get; set; }
        
        /// <summary>Telefon IMEI numarası (15 haneli benzersiz kod)</summary>
        public string? Imei { get; set; }
        
        /// <summary>Telefon markası (Apple, Samsung, Xiaomi vb.)</summary>
        public string? Marka { get; set; }
        
        /// <summary>Telefon modeli (iPhone 14, Galaxy S23 vb.)</summary>
        public string? Model { get; set; }
        
        /// <summary>Telefon rengi</summary>
        public string? Renk { get; set; }
        
        /// <summary>Garanti süresi (ay cinsinden, 0-24 arası)</summary>
        public int? GarantiAy { get; set; }
        
        /// <summary>Telefonun çıkış yılı</summary>
        public int? CikisYili { get; set; }
        
        /// <summary>Telefonu aldığımız fiyat (maliyet)</summary>
        public double? AlinanFiyat { get; set; }
        
        /// <summary>Telefon durumu (Stokta, Satıldı vb.)</summary>
        public string? Durum { get; set; }

        /// <summary>
        /// Alınan fiyatı Türkçe binlik ayırıcıyla formatlar
        /// </summary>
        /// <returns>Formatlanmış fiyat metni</returns>
        public string GetFormattedAlinanFiyat()
        {
            // Eğer AlinanFiyat null değilse, binlik ayırıcı ile formatla
            if (AlinanFiyat.HasValue)
            {
                return AlinanFiyat.Value.ToString("N0", CultureInfo.GetCultureInfo("tr-TR")); // Fiyatı binlik ayırıcı ile formatlar
            }
            else
            {
                return "Fiyat Bilgisi Yok"; // Eğer fiyat yoksa, bir mesaj döndürür
            }
        }
    }
}
