namespace RandaSports.Persistence.Seeders;

public sealed record CommentatorSeed(string FullName, string SportSlug, string[] Aliases);

/// <param name="Handle">YouTube kullanıcı adı. Kanal kimliği ilk taramada bundan çözülüyor.</param>
/// <param name="SportSlugs">
/// Kanalın kapsadığı branşlar. Modele ipucu olarak veriliyor; görüşün branşını
/// belirlemiyor. Genel spor kanallarında boş bırakılıyor.
/// </param>
public sealed record ChannelSeed(
    string Name,
    string Handle,
    string? YouTubeChannelId = null,
    string[]? SportSlugs = null);

/// <summary>
/// Başlangıç listesi. Kanallar yönetim ekranından düzenlenebiliyor; buradaki liste
/// yalnızca veritabanı bomboşken bir kez yazılıyor.
/// </summary>
public static class CommentatorSeedData
{
    public static readonly ChannelSeed[] Channels =
    [
        new("A Spor", "@aspor"),
        new("beIN SPORTS Türkiye", "@beinsportsturkiye"),
        new("TRT Spor", "@trtspor"),
        new("Sporx", "@sporx"),
        new("FutbolArena", "@futbolarena"),
        new("Socrates Dergi", "@socratesdergi"),
        new("Tribün Dergi", "@tribundergi"),
        new("Gol Dergisi", "@goldergisi"),

        // Yorumcu odaklı programlar — asıl görüş buralardan çıkıyor.
        new("Derin Futbol", "@DerinFutbol"),
        new("Kontraspor", "@kontraspor"),
        new("NOW Spor", "@nowsportr"),
        new("Libero TV", "@liberotv"),
        new("Mehmet Demirkol", "@MehmetDemirkolSocrates"),
        new("Ersin Düzen", "@ersinduzen"),

        // --- Branşa adanmış kanallar. Kanal kimlikleri doğrulandı ve hepsinin
        // --- beslemesi son 30 günde düzenli video veriyor.
        new("S Sport'ta UFC", "@ssporttaufc", "UCsPI6vY0Wl4nGqIV6X0080g", ["mma"]),
        new("8GEN", "@8GEN", "UCkyJbGEgInp0ztwBc4Bc12g", ["mma", "boks"]),
        new("Caner Akbaba", "@CanerAkbaba", "UCPfo9c-XFhyqMFpbJN_kbtQ", ["mma"]),
        new("Fight Digitale", "@FightDigitale", "UCd0tmX5IjJEVnLE5Aoar6Ng", ["mma", "boks"]),
        new("Bahadır Özen", "@BahadirOzen", "UCo2Ks2c79k9J-0V1z6q6TbA", ["mma"]),

        new("İlker Sırt", "@ilkersirt", "UCJgBwPB_3n5kbuvlIarMIpA", ["basketbol"]),
        new("Show Up", "@ShowUpBasket", "UCMSNluXmc8uWXT2YaOF_5PA", ["basketbol"]),

        new("Laki Analiz", "@LakiAnaliz", "UCT7Dymxec6tqPNV4czvYkfQ", ["voleybol"]),
        new("Voleybol Akademi", "@VoleybolAkademi", "UCRiBKK1gCOb0fmKKd215jUw", ["voleybol"]),

        new("HTalks", "@HTalks", "UCFr6uAPwrG040QAWDKY0nnA", ["amerikan-futbolu", "futbol", "basketbol"]),
        new("NFLTR", "@nfltrtv", "UC5OUm2rkoJPFU77xlq8X21g", ["amerikan-futbolu"]),

        new("Kubilay Vergili", "@KubilayVergili", "UCSt8UeSh2YJJI9gnPw5lN0w", ["formula-1", "motogp"]),
        new("Yiğit Tezcan", "@YigitTezcan", "UCt3fP0lr6l8dK7pmlzoxRcg", ["formula-1"]),

        new("Grand Slam Racing", "@GrandSlamRacing", "UCSWIRscL8S3sDqAko7T2ifw", ["motogp"]),
        new("Furkan Sönmez", "@FurkanSonmez", "UCONmpCpaOqBZFByYuUHc6Lg", ["motogp"]),
        new("S Sport'ta MotoGP", "@ssporttamotogp", "UCCOLmmp8DDErGVLzs__GsgQ", ["motogp"]),

        new("SPORTSNET", "@SPORTSNETtr", "UCVhibwHk4WKw4leUt6JfRLg", ["buz-hokeyi"]),

        new("Tennis Emre Bakir", "@TennisEmreBakir", "UCzDahC0WdGdXMoz_qIgbfdg", ["tenis"]),
        new("Yunus Dilber", "@YunusDilber", "UCsgsiUwQun1gMw2pM_G877A", ["tenis"])
    ];

    /// <summary>
    /// Konuşmacı doğrulaması bu listeye bakıyor: modelin verdiği isim burada yoksa
    /// görüş isme bağlanmıyor, "kaynak belirsiz" olarak onaya düşüyor.
    /// </summary>
    public static readonly CommentatorSeed[] Commentators =
    [
        new("Rıdvan Dilmen", "futbol", ["Dilmen"]),
        new("Erman Toroğlu", "futbol", ["Toroğlu", "Toroglu"]),
        new("Ahmet Çakar", "futbol", ["Çakar", "Cakar"]),
        new("Serdar Ali Çelikler", "futbol", ["Serdar Ali", "Çelikler", "Celikler"]),
        new("Mehmet Demirkol", "futbol", ["Demirkol"]),
        new("Nihat Kahveci", "futbol", ["Kahveci", "Nihat Kahveci"]),
        new("Ersin Düzen", "futbol", ["Düzen", "Duzen"]),
        new("Ümit Özat", "futbol", ["Özat", "Ozat"]),
        new("Sabri Ugan", "futbol", ["Ugan"]),
        new("Önder Özen", "futbol", ["Önder Özen", "Onder Ozen"]),
        new("Uğur Meleke", "futbol", ["Meleke"]),
        new("Fatih Doğan", "futbol", ["Fatih Doğan", "Fatih Dogan"]),
        new("Cem Dizdar", "futbol", ["Dizdar"]),
        new("Gürcan Bilgiç", "futbol", ["Bilgiç", "Bilgic"]),
        new("Fırat Aydınus", "futbol", ["Aydınus", "Aydinus"]),
        new("Metin Tekin", "futbol", ["Metin Tekin"]),
        new("Ali Ece", "futbol", ["Ali Ece"]),
        new("Uğur Karakullukçu", "futbol", ["Karakullukçu", "Karakullukcu"]),
        new("Abdülkerim Durmaz", "futbol", ["Durmaz", "Abdülkerim"]),
        new("Emek Ege", "futbol", ["Emek Ege"]),
        new("Ilgaz Çınar", "futbol", ["Ilgaz", "Çınar", "Cinar"]),
        new("Fuat Akdağ", "futbol", ["Akdağ", "Akdag"]),
        new("Erman Özgür", "futbol", ["Erman Özgür", "Erman Ozgur"]),
        new("Ümit Karan", "futbol", ["Ümit Karan", "Umit Karan"]),
        new("Batuhan Karadeniz", "futbol", ["Karadeniz", "Batuhan"]),
        new("Serhat Ulueren", "futbol", ["Ulueren"]),
        new("Şansal Büyüka", "futbol", ["Büyüka", "Buyuka"]),
        new("Tümer Metin", "futbol", ["Tümer Metin", "Tumer Metin"]),
        new("Tugay Kerimoğlu", "futbol", ["Kerimoğlu", "Kerimoglu"]),
        new("Murat Murathanoğlu", "basketbol", ["Murathanoğlu", "Murathanoglu"]),
        new("Kaya Peker", "basketbol", ["Peker"]),
        new("Ömer Onan", "basketbol", ["Onan"]),
        new("Haldun Domaç", "basketbol", ["Domaç", "Domac"])
    ];
}
