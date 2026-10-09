namespace RandaSports.Worker.Workers;

public sealed class OpinionExtractionOptions
{
    public const string SectionName = "OpinionExtraction";

    public bool Enabled { get; set; } = true;

    /// <summary>İki tur arasındaki bekleme (dakika).</summary>
    public int IntervalMinutes { get; set; } = 20;

    /// <summary>
    /// Bir turda en fazla kaç video işlenecek. Video analizi modelin en pahalı
    /// işlemi; ücretsiz kotayı tüketmemek için küçük tutuluyor.
    /// </summary>
    public int BatchSize { get; set; } = 2;

    /// <summary>Bir video en fazla kaç kez denenecek.</summary>
    public int MaxAttempts { get; set; } = 2;

    /// <summary>İki video arasındaki bekleme (saniye).</summary>
    public int DelaySeconds { get; set; } = 20;

    /// <summary>Görüş, bu kadar günlük haberlerle eşleştirilmeye çalışılıyor.</summary>
    public int StoryMatchDays { get; set; } = 4;

    /// <summary>
    /// Bu süreyi aşan video dilimlenerek işleniyor (dakika). Altındakiler tek
    /// çağrıda gidiyor — shorts ve kısa kliplerde zaten kayma yok.
    /// </summary>
    public int SegmentThresholdMinutes { get; set; } = 12;

    /// <summary>
    /// Dilim uzunluğu (dakika). Model ne kadar kısa süre takip ederse zaman damgası
    /// o kadar isabetli. Kırpma orantılı olduğu için toplam token maliyeti videoyu
    /// bir kez işlemekle aşağı yukarı aynı kalıyor; artan şey çağrı sayısı.
    /// </summary>
    public int SegmentMinutes { get; set; } = 8;

    /// <summary>
    /// Dilimler arası bindirme (saniye). Dilim sınırına denk gelen konuşma ikiye
    /// bölünmesin diye; oluşan kopyalar sonradan ayıklanıyor.
    /// </summary>
    public int SegmentOverlapSeconds { get; set; } = 15;

    /// <summary>
    /// Bir videodan yayına alınacak en fazla görüş. Dilimlerin her biri kendi
    /// görüşlerini döndürdüğü için bu sınır olmadan uzun bir yayından on beş görüş
    /// çıkıyor ve akış doluyor. En çarpıcı birkaçı yetiyor.
    /// </summary>
    public int MaxOpinionsPerVideo { get; set; } = 3;

    /// <summary>
    /// Bir videoya en fazla kaç Gemini çağrısı yapılacak. İki saatlik bir yayın
    /// 8 dakikalık dilimlerle on beş çağrı demek ve günlük kota tek videoya gidiyor.
    /// Sınır aşılırsa dilim uzatılıyor; video yine baştan sona işleniyor.
    /// </summary>
    public int MaxSegmentsPerVideo { get; set; } = 8;

    /// <summary>
    /// Kısa videolar önce işlensin mi. Shorts genelde tek yorumcunun tek görüşü,
    /// konuşmacının adı da başlıkta; aynı kotayla kat kat fazla görüş çıkıyor.
    /// Uzun programları öne almak istersen false yap.
    /// </summary>
    public bool PreferShortVideos { get; set; } = true;
}
