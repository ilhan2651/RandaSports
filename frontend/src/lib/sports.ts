const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

type ApiResult<T> = {
  success: boolean;
  data?: T;
  messages: string[];
};

export type TeamSummary = {
  name: string;
  slug: string;
  logoUrl: string | null;
};

export type FixtureSide = TeamSummary & { score: number | null };

export type Fixture = {
  id: string;
  slug: string;
  startTime: string;
  status: "Scheduled" | "Live" | "Finished" | "Postponed" | "Cancelled";
  statusDetail: string | null;
  round: string | null;
  venue: string | null;
  home: FixtureSide;
  away: FixtureSide;
};

export type StandingRow = {
  rank: number;
  team: TeamSummary;
  played: number;
  won: number;
  drawn: number;
  lost: number;
  points: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  form: string | null;
};

export type Standings = {
  competitionName: string;
  competitionSlug: string;
  seasonName: string | null;
  rows: StandingRow[];
};

export type AthleteListItem = {
  fullName: string;
  slug: string;
  photoUrl: string | null;
  position: string | null;
  shirtNumber: number | null;
  goals: number | null;
  assists: number | null;
};

export type TeamDetail = {
  id: string;
  name: string;
  slug: string;
  logoUrl: string | null;
  country: string | null;
  standing: StandingRow | null;
  squad: AthleteListItem[];
  recentFixtures: Fixture[];
  upcomingFixtures: Fixture[];
};

export type AthleteCompetition = {
  leagueName: string | null;
  teamName: string | null;
  isNationalTeam: boolean;
  appearances: number;
  minutes: number;
  goals: number;
  assists: number;
  shotsTotal: number;
  shotsOn: number;
  passesTotal: number;
  passesKey: number;
  passAccuracy: number | null;
  duelsTotal: number;
  duelsWon: number;
  dribblesAttempts: number;
  dribblesSuccess: number;
  yellowCards: number;
  redCards: number;
  rating: number | null;
};

export type AthleteDetail = {
  id: string;
  fullName: string;
  slug: string;
  photoUrl: string | null;
  position: string | null;
  shirtNumber: number | null;
  nationality: string | null;
  birthDate: string | null;
  age: number | null;
  birthPlace: string | null;
  heightCm: number | null;
  weightKg: number | null;
  team: TeamSummary | null;
  appearances: number | null;
  goals: number | null;
  assists: number | null;
  minutesPlayed: number | null;
  rating: number | null;
  statsSeason: number | null;
  competitions: AthleteCompetition[];
};

async function get<T>(path: string, fallback: T): Promise<T> {
  try {
    const response = await fetch(`${API_BASE}${path}`, { next: { revalidate: 300 } });
    if (!response.ok) return fallback;

    const result = (await response.json()) as ApiResult<T>;
    return result.data ?? fallback;
  } catch {
    return fallback;
  }
}

export function getStandings(sport = "futbol"): Promise<Standings | null> {
  return get<Standings | null>(`/api/standings?sport=${sport}`, null);
}

export function getFixtures(params: { sport?: string; team?: string } = {}): Promise<Fixture[]> {
  const query = new URLSearchParams({ sport: params.sport ?? "futbol" });
  if (params.team) query.set("team", params.team);

  return get<Fixture[]>(`/api/fixtures?${query}`, []);
}

export function getTeam(slug: string): Promise<TeamDetail | null> {
  return get<TeamDetail | null>(`/api/teams/${encodeURIComponent(slug)}`, null);
}

export function getAthlete(slug: string): Promise<AthleteDetail | null> {
  return get<AthleteDetail | null>(`/api/athletes/${encodeURIComponent(slug)}`, null);
}
