namespace RandaSports.Domain.Enums;

/// <summary>
/// Alıntının videoda gerçekten böyle söylenip söylenmediğine dair makine ön kontrolü.
///
/// Bu, insanın onayının yerine geçmiyor: onay ekranına giren kişinin her görüş için
/// videoyu baştan açmasını engelliyor, kararı ona bırakıyor. Alanın karşılığı
/// <see cref="Entities.Opinion.IsQuoteVerified"/> DEĞİL — orası insanın imzası.
/// </summary>
public enum QuoteCheckResult
{
    /// <summary>Henüz bakılmadı.</summary>
    NotChecked,

    /// <summary>Kelimesi kelimesine aynı.</summary>
    Verbatim,

    /// <summary>
    /// Anlam aynı ama kelimeler tutmuyor. Yayına girmemeli: alıntı birebir olmak
    /// zorunda, yakını "şöyle demiş" değil "şöyle demek istemiş" olur.
    /// </summary>
    Paraphrased,

    /// <summary>Söylenen şey bu değil.</summary>
    Different,

    /// <summary>Alıntı doğru ama söyleyen başkası.</summary>
    WrongSpeaker,

    /// <summary>Verilen pencerede böyle bir söz duyulmadı — damga kaymış olabilir.</summary>
    NotHeard,

    /// <summary>Modele ulaşılamadı ya da cevabı okunamadı; yeniden denenebilir.</summary>
    Failed
}
