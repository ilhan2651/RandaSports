namespace RandaSports.Worker.Workers;

public sealed class SportsSyncOptions
{
    public const string SectionName = "SportsSync";

    public bool Enabled { get; set; } = true;

    /// <summary>İki tur arasındaki bekleme (dakika).</summary>
    public int IntervalMinutes { get; set; } = 60;

    /// <summary>Lig, sezon ve takım listesi bu sıklıkla tazelenir (saat).</summary>
    public int ReferenceSyncHours { get; set; } = 24;

    /// <summary>Fikstür ve sonuçlar bu sıklıkla tazelenir (saat).</summary>
    public int FixtureSyncHours { get; set; } = 6;

    /// <summary>Puan durumu bu sıklıkla tazelenir (saat).</summary>
    public int StandingsSyncHours { get; set; } = 12;

    /// <summary>Fikstürde geriye ve ileriye kaç gün bakılır.</summary>
    public int FixtureDaysBack { get; set; } = 7;
    public int FixtureDaysAhead { get; set; } = 14;

    /// <summary>Kadrolar bu sıklıkla tazelenir (gün).</summary>
    public int SquadSyncDays { get; set; } = 7;

    /// <summary>Oyuncu istatistikleri bu sıklıkla tazelenir (gün).</summary>
    public int StatsSyncDays { get; set; } = 3;

    /// <summary>Bir turda en fazla kaç takımın kadrosu çekilir.</summary>
    public int TeamsPerTick { get; set; } = 2;

    /// <summary>Bir turda en fazla kaç oyuncunun istatistiği çekilir.</summary>
    public int AthletesPerTick { get; set; } = 3;

    /// <summary>Canlı skor için kotada bu kadar istek her zaman ayrı tutulur.</summary>
    public int ReservedRequests { get; set; } = 30;
}
