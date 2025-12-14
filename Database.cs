using System;
using System.Data.SQLite;
using System.IO;

namespace TelefonSatısApp
{
    /// <summary>
    /// Veritabanı işlemleri sınıfı - SQLite veritabanı bağlantısı ve tablo oluşturma işlemleri
    /// </summary>
    public static class Database
    {
        // Uygulamanın çalıştığı klasör yolu
        private static readonly string baseDir =
            AppDomain.CurrentDomain.BaseDirectory;

        // Veritabanı dosyalarının saklanacağı klasör yolu
        private static readonly string dataDir =
            Path.Combine(baseDir, "Data");

        // Veritabanı dosyasının tam yolu
        private static readonly string dbPath =
            Path.Combine(dataDir, "telefonlar.db");

        // SQLite bağlantı dizesi
        private static readonly string connectionString =
            $"Data Source={dbPath};Version=3;";

        /// <summary>
        /// Yeni bir veritabanı bağlantısı döndürür
        /// </summary>
        /// <returns>SQLite bağlantı nesnesi</returns>
        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        /// <summary>
        /// Veritabanını başlatır - Klasörleri oluşturur, tabloları hazırlar ve örnek veriler ekler
        /// </summary>
        public static void Initialize()
        {
            // Data klasörünü oluştur (yoksa)
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);

            // Veritabanı dosyasının var olup olmadığını kontrol et
            bool isNewDatabase = !File.Exists(dbPath);
            if (isNewDatabase)
                SQLiteConnection.CreateFile(dbPath);

            // Veritabanına bağlan ve tabloları oluştur
            using (var conn = GetConnection())
            {
                conn.Open();

                // Telefonlar tablosunu oluştur - Stok bilgilerini tutar
                string createTelefonlarTableSql = @"
                CREATE TABLE IF NOT EXISTS Telefonlar (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Imei TEXT NOT NULL,
                    Model TEXT NOT NULL,
                    Marka TEXT NOT NULL,
                    Renk TEXT,
                    GarantiAy INTEGER,  -- 0 ile 24 ay arası
                    CikisYili INTEGER,
                    AlinanFiyat REAL NOT NULL,
                    Durum TEXT
                );
            ";

                using (var cmd = new SQLiteCommand(createTelefonlarTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Peşin Satışlar tablosunu oluştur - Nakit satış kayıtlarını tutar
                string createPesinSatislarTableSql = @"
                CREATE TABLE IF NOT EXISTS PesinSatislar (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TelefonId INTEGER NOT NULL,
                    MusteriAd TEXT NOT NULL,
                    MusteriSoyad TEXT NOT NULL,
                    MusteriTelefon TEXT NOT NULL,
                    SatisFiyati REAL NOT NULL,
                    Kar REAL,
                    Tarih DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY(TelefonId) REFERENCES Telefonlar(Id)
                );
            ";

                using (var cmd = new SQLiteCommand(createPesinSatislarTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Taksitli Satışlar tablosunu oluştur - Taksitli satış kayıtlarını tutar
                string createTaksitliSatislarTableSql = @"
                CREATE TABLE IF NOT EXISTS TaksitliSatislar (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TelefonId INTEGER NOT NULL,
                    MusteriAd TEXT NOT NULL,
                    MusteriSoyad TEXT NOT NULL,
                    Telefon1 TEXT NOT NULL,
                    Telefon2 TEXT,
                    TaksitSayisi INTEGER NOT NULL CHECK(TaksitSayisi BETWEEN 1 AND 6),
                    Tarih DATETIME DEFAULT CURRENT_TIMESTAMP
                );
            ";

                using (var cmd = new SQLiteCommand(createTaksitliSatislarTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Mevcut tabloda eksik kolonlar varsa ekle (ALTER TABLE ile, varsa hata alırız; hatayı yok sayıyoruz)
                TryAddColumn(conn, "TaksitliSatislar", "TelefonId INTEGER");
                TryAddColumn(conn, "TaksitliSatislar", "SatisFiyati REAL NOT NULL DEFAULT 0");
                TryAddColumn(conn, "TaksitliSatislar", "AylikOdeme REAL NOT NULL DEFAULT 0");
                TryAddColumn(conn, "TaksitliSatislar", "Notlar TEXT");
                TryAddColumn(conn, "TaksitliSatislar", "SatisTarihi DATETIME");
                TryAddColumn(conn, "TaksitliSatislar", "TaksitBaslangicTarihi DATETIME");
                TryAddColumn(conn, "TaksitliSatislar", "AlinanFiyat REAL");
                TryAddColumn(conn, "PesinSatislar", "AlinanFiyat REAL");
                TryAddColumn(conn, "PesinSatislar", "MusteriAdSoyad TEXT");
                TryAddColumn(conn, "PesinSatislar", "Marka TEXT");
                TryAddColumn(conn, "PesinSatislar", "Model TEXT");
                TryAddColumn(conn, "PesinSatislar", "Durum TEXT");
                TryAddColumn(conn, "TaksitliSatislar", "MusteriAdSoyad TEXT");
                TryAddColumn(conn, "TaksitliSatislar", "OnOdeme REAL DEFAULT 0");

                // Taksit ödemeleri tablosunu oluştur - Her taksit ödemesini ayrı ayrı takip eder
                string createTaksitOdemeleriTableSql = @"
                CREATE TABLE IF NOT EXISTS TaksitOdemeleri (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TaksitliSatisId INTEGER NOT NULL,
                    TaksitNo INTEGER NOT NULL,
                    VadeTarihi DATETIME NOT NULL,
                    Odendi INTEGER NOT NULL DEFAULT 0,
                    OdemeTarihi DATETIME,
                    FOREIGN KEY(TaksitliSatisId) REFERENCES TaksitliSatislar(Id)
                );
            ";

                using (var cmd = new SQLiteCommand(createTaksitOdemeleriTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Telefon Alışları tablosunu oluştur - Gider takibi için telefon alım kayıtları
                string createTelefonAlislariTableSql = @"
                CREATE TABLE IF NOT EXISTS TelefonAlislari (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TelefonId INTEGER NOT NULL,
                    Imei TEXT NOT NULL,
                    Model TEXT NOT NULL,
                    Marka TEXT NOT NULL,
                    Renk TEXT,
                    AlinanFiyat REAL NOT NULL,
                    Tarih DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY(TelefonId) REFERENCES Telefonlar(Id)
                );
            ";

                using (var cmd = new SQLiteCommand(createTelefonAlislariTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Eğer yeni veritabanı ise, örnek telefonları ekle
                if (isNewDatabase)
                {
                    AddSamplePhones(conn);
                }
            }
        }

        /// <summary>
        /// Yeni veritabanına örnek telefon verilerini ekler
        /// </summary>
        /// <param name="conn">Veritabanı bağlantısı</param>
        private static void AddSamplePhones(SQLiteConnection conn)
        {
            var phones = new[]
            {
                new { Imei = "123456789012345", Model = "iPhone 14", Marka = "Apple", Renk = "Siyah", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 25000.0, Durum = "Stokta" },
                new { Imei = "123456789012346", Model = "iPhone 13", Marka = "Apple", Renk = "Beyaz", GarantiAy = 12, CikisYili = 2021, AlinanFiyat = 22000.0, Durum = "Stokta" },
                new { Imei = "123456789012347", Model = "Galaxy S23", Marka = "Samsung", Renk = "Siyah", GarantiAy = 24, CikisYili = 2023, AlinanFiyat = 20000.0, Durum = "Stokta" },
                new { Imei = "123456789012348", Model = "Galaxy S22", Marka = "Samsung", Renk = "Beyaz", GarantiAy = 24, CikisYili = 2022, AlinanFiyat = 18000.0, Durum = "Stokta" },
                new { Imei = "123456789012349", Model = "P50 Pro", Marka = "Huawei", Renk = "Altın", GarantiAy = 18, CikisYili = 2021, AlinanFiyat = 15000.0, Durum = "Stokta" },
                new { Imei = "123456789012350", Model = "Mi 12", Marka = "Xiaomi", Renk = "Mavi", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 12000.0, Durum = "Stokta" },
                new { Imei = "123456789012351", Model = "OnePlus 10", Marka = "OnePlus", Renk = "Yeşil", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 14000.0, Durum = "Stokta" },
                new { Imei = "123456789012352", Model = "Pixel 7", Marka = "Google", Renk = "Siyah", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 16000.0, Durum = "Stokta" },
                new { Imei = "123456789012353", Model = "iPhone 12", Marka = "Apple", Renk = "Kırmızı", GarantiAy = 12, CikisYili = 2020, AlinanFiyat = 19000.0, Durum = "Stokta" },
                new { Imei = "123456789012354", Model = "Galaxy A54", Marka = "Samsung", Renk = "Mor", GarantiAy = 24, CikisYili = 2023, AlinanFiyat = 8000.0, Durum = "Stokta" },
                new { Imei = "123456789012355", Model = "Redmi Note 12", Marka = "Xiaomi", Renk = "Gri", GarantiAy = 12, CikisYili = 2023, AlinanFiyat = 6000.0, Durum = "Stokta" },
                new { Imei = "123456789012356", Model = "Find X5", Marka = "Oppo", Renk = "Beyaz", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 13000.0, Durum = "Stokta" },
                new { Imei = "123456789012357", Model = "V30", Marka = "Vivo", Renk = "Siyah", GarantiAy = 12, CikisYili = 2023, AlinanFiyat = 11000.0, Durum = "Stokta" },
                new { Imei = "123456789012358", Model = "Nord 2T", Marka = "OnePlus", Renk = "Mavi", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 9000.0, Durum = "Stokta" },
                new { Imei = "123456789012359", Model = "iPhone SE", Marka = "Apple", Renk = "Beyaz", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 13000.0, Durum = "Stokta" },
                new { Imei = "123456789012360", Model = "Galaxy M54", Marka = "Samsung", Renk = "Yeşil", GarantiAy = 24, CikisYili = 2023, AlinanFiyat = 7500.0, Durum = "Stokta" },
                new { Imei = "123456789012361", Model = "Poco F4", Marka = "Xiaomi", Renk = "Siyah", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 8500.0, Durum = "Stokta" },
                new { Imei = "123456789012362", Model = "Nova 10", Marka = "Huawei", Renk = "Pembe", GarantiAy = 18, CikisYili = 2022, AlinanFiyat = 10000.0, Durum = "Stokta" },
                new { Imei = "123456789012363", Model = "Pixel 6a", Marka = "Google", Renk = "Gri", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 12000.0, Durum = "Stokta" },
                new { Imei = "123456789012364", Model = "Reno 8", Marka = "Oppo", Renk = "Altın", GarantiAy = 12, CikisYili = 2022, AlinanFiyat = 9500.0, Durum = "Stokta" }
            };

            foreach (var phone in phones)
            {
                string sql = @"INSERT INTO Telefonlar (Imei, Model, Marka, Renk, GarantiAy, CikisYili, AlinanFiyat, Durum) 
                               VALUES (@Imei, @Model, @Marka, @Renk, @GarantiAy, @CikisYili, @AlinanFiyat, @Durum)";
                
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Imei", phone.Imei);
                    cmd.Parameters.AddWithValue("@Model", phone.Model);
                    cmd.Parameters.AddWithValue("@Marka", phone.Marka);
                    cmd.Parameters.AddWithValue("@Renk", phone.Renk);
                    cmd.Parameters.AddWithValue("@GarantiAy", phone.GarantiAy);
                    cmd.Parameters.AddWithValue("@CikisYili", phone.CikisYili);
                    cmd.Parameters.AddWithValue("@AlinanFiyat", phone.AlinanFiyat);
                    cmd.Parameters.AddWithValue("@Durum", phone.Durum);
                    
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Tabloya yeni kolon eklemeye çalışır - Hata alırsa (kolon zaten varsa) yok sayar
        /// </summary>
        /// <param name="conn">Veritabanı bağlantısı</param>
        /// <param name="table">Tablo adı</param>
        /// <param name="columnDefinition">Kolon tanımı</param>
        private static void TryAddColumn(SQLiteConnection conn, string table, string columnDefinition)
        {
            try
            {
                using (var cmd = new SQLiteCommand($"ALTER TABLE {table} ADD COLUMN {columnDefinition};", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Kolon zaten varsa veya tablo yoksa hata alırız, yoksay.
            }
        }
    }

}
