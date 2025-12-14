using System;

namespace TelefonSatısApp
{
    /// <summary>
    /// Taksitli satış model sınıfı - Taksitle yapılan telefon satışlarının bilgilerini tutar
    /// </summary>
    public class TaksitliSatis
    {
        /// <summary>Taksitli satış benzersiz kimlik numarası</summary>
        public int Id { get; set; }
        
        /// <summary>Satılan telefonun kimlik numarası</summary>
        public int TelefonId { get; set; }
        
        /// <summary>Müşteri adı</summary>
        public string MusteriAd { get; set; } = string.Empty;
        
        /// <summary>Müşteri soyadı</summary>
        public string MusteriSoyad { get; set; } = string.Empty;
        
        /// <summary>Müşteri ad ve soyadının birleşik hali</summary>
        public string MusteriAdSoyad { get; set; } = string.Empty;
        
        /// <summary>Müşteri birincil telefon numarası</summary>
        public string Telefon1 { get; set; } = string.Empty;
        
        /// <summary>Müşteri ikincil telefon numarası (opsiyonel)</summary>
        public string? Telefon2 { get; set; }
        
        /// <summary>Taksit sayısı (1-6 ay arası)</summary>
        public int TaksitSayisi { get; set; }
        
        /// <summary>Toplam satış fiyatı</summary>
        public double SatisFiyati { get; set; }
        
        /// <summary>Peşin ödenen miktar</summary>
        public double OnOdeme { get; set; }
        
        /// <summary>Aylık ödeme miktarı</summary>
        public double AylikOdeme { get; set; }
        
        /// <summary>Kayıt tarihi</summary>
        public DateTime Tarih { get; set; }
        
        /// <summary>Satış tarihi</summary>
        public DateTime SatisTarihi { get; set; }
        
        /// <summary>Taksit ödemelerinin başlangıç tarihi</summary>
        public DateTime TaksitBaslangicTarihi { get; set; }
        
        /// <summary>Satılan telefonun markası</summary>
        public string Marka { get; set; } = string.Empty;
        
        /// <summary>Satılan telefonun modeli</summary>
        public string Model { get; set; } = string.Empty;
        
        /// <summary>Telefonun durumu (Sıfır/İkinci El)</summary>
        public string Durum { get; set; } = string.Empty;
    }
}

