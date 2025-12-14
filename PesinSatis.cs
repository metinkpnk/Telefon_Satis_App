using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelefonSatısApp
{
    /// <summary>
    /// Peşin satış model sınıfı - Nakit olarak yapılan telefon satışlarının bilgilerini tutar
    /// </summary>
    public class PesinSatis
    {
        /// <summary>Müşteri adı</summary>
        public string? MusteriAd { get; set; }
        
        /// <summary>Müşteri soyadı</summary>
        public string? MusteriSoyad { get; set; }
        
        /// <summary>Müşteri ad ve soyadının birleşik hali</summary>
        public string? MusteriAdSoyad { get; set; }
        
        /// <summary>Müşteri telefon numarası</summary>
        public string? MusteriTelefon { get; set; }
        
        /// <summary>Satılan telefonun markası</summary>
        public string? Marka { get; set; }
        
        /// <summary>Satılan telefonun modeli</summary>
        public string? Model { get; set; }
        
        /// <summary>Telefonun durumu (Sıfır/İkinci El)</summary>
        public string? Durum { get; set; }
        
        /// <summary>Müşteriye satış yapılan fiyat</summary>
        public double SatisFiyati { get; set; }
        
        /// <summary>Satıştan elde edilen kar (Satış Fiyatı - Alış Fiyatı)</summary>
        public double Kar { get; set; }
        
        /// <summary>Satış tarihi</summary>
        public DateTime? Tarih { get; set; }
    }
}
