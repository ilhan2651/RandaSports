import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { savePreferences } from "@/app/hesap/actions";
import { EmptyState } from "@/components/empty-state";
import { EntityAvatar } from "@/components/entity-avatar";
import { HeroStory } from "@/components/hero-story";
import { OnboardingModal } from "@/components/onboarding-modal";
import { OpinionDayGroups } from "@/components/opinion-day-groups";
import { Reveal } from "@/components/reveal";
import { SectionTitle } from "@/components/section-title";
import { StoryCard } from "@/components/story-card";
import { StoryListItem } from "@/components/story-list-item";
import { getFeed, getMe, getOnboardingOptions, type Feed, type Followed } from "@/lib/me";

export const metadata: Metadata = {
  title: "Akışım",
  robots: { index: false, follow: false },
};

export default async function FeedPage() {
  const me = await getMe();

  // Jeton düşmüş ya da hiç yok: girişe alıyoruz, dönüşte buraya geliyor.
  if (!me) redirect("/giris?devam=/akis");

  const feed = await getFeed();

  if (!feed)
    return (
      <EmptyState
        title="Akış yüklenemedi"
        hint="API'ye ulaşılamadı. Birkaç saniye sonra sayfayı yenile."
      />
    );

  // Sihirbaz giriş sonrası ilk durağın kendisi: akış burada, tercih yoksa önce o soruluyor.
  const secenekler = me.onboardingCompleted ? null : await getOnboardingOptions();

  const { stories, opinions } = feed;
  const [hero, ...rest] = stories;
  const yan = rest.slice(0, 5);
  const izgara = rest.slice(5, 14);
  const devam = rest.slice(14);

  return (
    <div className="pt-6">
      <FeedHeader feed={feed} />

      {!feed.hasPreferences ? (
        <BosDurum
          baslik="Akışın henüz boş"
          metin="Hangi branşları, yorumcuları ve takımları takip ettiğini seç; buradaki haberler ve görüşler yalnızca onlardan gelir."
          eylem="Tercihleri seç"
        />
      ) : stories.length === 0 && opinions.length === 0 ? (
        <BosDurum
          baslik="Takip ettiklerinden henüz içerik yok"
          metin="Seçimlerin kaydedildi ama bu başlıklarda yeni haber ya da görüş çıkmadı. Takip listeni genişletmek akışı hemen doldurur."
          eylem="Takip listemi düzenle"
          sakin
        />
      ) : (
        <>
          {hero && (
            <section className="mt-6 grid gap-6 lg:grid-cols-[1.9fr_1fr]">
              <Reveal>
                <HeroStory story={hero} />
              </Reveal>

              <Reveal delay={0.1}>
                <div className="space-y-5">
                  {yan.length > 0 && (
                    <div className="rounded-2xl border border-line bg-surface p-5">
                      <h2 className="font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
                        Son gelenler
                      </h2>
                      <div className="mt-2">
                        {yan.map((story, index) => (
                          <StoryListItem key={story.id} story={story} index={index} />
                        ))}
                      </div>
                    </div>
                  )}

                  <SportBreakdown feed={feed} />
                </div>
              </Reveal>
            </section>
          )}

          {izgara.length > 0 && (
            <section className="mt-16">
              <SectionTitle>Takip ettiklerinden</SectionTitle>
              <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
                {izgara.map((story, index) => (
                  <Reveal key={story.id} delay={(index % 3) * 0.08}>
                    <StoryCard story={story} />
                  </Reveal>
                ))}
              </div>
            </section>
          )}

          {devam.length > 0 && (
            <section className="mt-16">
              <SectionTitle>Daha fazla</SectionTitle>
              <div className="grid gap-x-10 md:grid-cols-2">
                {devam.map((story, index) => (
                  <StoryListItem key={story.id} story={story} index={index} />
                ))}
              </div>
            </section>
          )}

          {opinions.length > 0 && (
            <section className="mt-16">
              <div className="mb-6 flex flex-wrap items-baseline justify-between gap-x-4 gap-y-1 border-b border-line pb-3">
                <h2 className="font-display text-2xl font-extrabold uppercase tracking-tight">
                  Takip ettiklerin ne dedi
                </h2>
                <Link
                  href="/yorumcular"
                  className="shrink-0 font-display text-xs font-bold uppercase tracking-widest text-amber underline-offset-4 outline-none transition-colors hover:underline focus-visible:underline"
                >
                  Tüm görüşler
                </Link>
                <p className="w-full text-xs text-fog">
                  {feed.commentators.length > 0
                    ? "Takip ettiğin yorumcuların, takip ettiğin branş ve takımlar hakkındaki görüşleri."
                    : "Hiç yorumcu seçmediğin için takip ettiğin branş ve takımlardaki tüm görüşler geliyor."}
                </p>
              </div>

              <OpinionDayGroups opinions={opinions} />
            </section>
          )}
        </>
      )}

      {secenekler && <OnboardingModal options={secenekler} kaydet={savePreferences} />}
    </div>
  );
}

/**
 * Başlık ve altında tek satırlık tercih şeridi. Takip listesi daha önce sağda koca bir
 * kart kaplıyordu; asıl iş akışı okumak, tercihler yalnızca "neden bunlar geliyor"u
 * açıklayan küçük bir not.
 */
function FeedHeader({ feed }: { feed: Feed }) {
  const hepsi: { item: Followed; yuvarlak: boolean; href: string }[] = [
    ...feed.sports.map((item) => ({ item, yuvarlak: false, href: `/${item.slug}` })),
    ...feed.commentators.map((item) => ({ item, yuvarlak: true, href: `/yorumcu/${item.slug}` })),
    ...feed.teams.map((item) => ({ item, yuvarlak: false, href: `/takim/${item.slug}` })),
  ];

  return (
    <header className="border-b border-line pb-4">
      <div className="flex flex-wrap items-end justify-between gap-x-4 gap-y-2">
        <h1 className="font-display text-3xl font-extrabold uppercase tracking-tight sm:text-4xl">
          Akışım
        </h1>

        <Link
          href="/hesap"
          className="font-display text-xs font-bold uppercase tracking-widest text-fog underline-offset-4 outline-none transition-colors hover:text-chalk hover:underline focus-visible:underline"
        >
          Tercihleri değiştir
        </Link>
      </div>

      {hepsi.length > 0 && (
        // Yatay kaydırılabilir: on beş seçimde bile başlık alanı büyümüyor.
        <div className="mt-3 flex gap-1.5 overflow-x-auto pb-1 [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
          {hepsi.map(({ item, yuvarlak, href }) => (
            <Link
              key={item.id}
              href={href}
              className="flex shrink-0 items-center gap-1.5 rounded-full border border-line bg-surface py-1 pl-1 pr-2.5 outline-none transition-colors hover:border-amber/40 focus-visible:border-amber"
            >
              <EntityAvatar
                name={item.name}
                src={item.imageUrl}
                size={18}
                className={yuvarlak ? "" : "!rounded-full"}
              />
              <span className="whitespace-nowrap font-display text-[11px] font-bold uppercase tracking-wide text-fog">
                {item.name}
              </span>
            </Link>
          ))}
        </div>
      )}
    </header>
  );
}

function BosDurum({
  baslik,
  metin,
  eylem,
  sakin = false,
}: {
  baslik: string;
  metin: string;
  eylem: string;
  sakin?: boolean;
}) {
  return (
    <div className="mt-10 rounded-2xl border border-dashed border-line bg-surface px-6 py-16 text-center">
      <h2 className="font-display text-2xl font-bold uppercase tracking-tight">{baslik}</h2>
      <p className="mx-auto mt-3 max-w-md text-sm text-fog">{metin}</p>
      <Link
        href="/hesap"
        className={`mt-6 inline-block rounded-lg border px-5 py-2.5 font-display text-xs font-bold uppercase tracking-widest outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
          sakin
            ? "border-line text-fog hover:border-amber/40 hover:text-chalk"
            : "border-amber/40 bg-amber/10 text-amber hover:bg-amber/20"
        }`}
      >
        {eylem}
      </Link>
    </div>
  );
}

/**
 * Akıştaki haber ve görüşlerin branşa dağılımı. Tek bir branş akışı doldurduğunda
 * bunu görmek, takip listesini genişletmek için en net işaret.
 */
function SportBreakdown({ feed }: { feed: Feed }) {
  const sayac = new Map<string, number>();

  for (const slug of [
    ...feed.stories.map((x) => x.sportSlug),
    ...feed.opinions.map((x) => x.sportSlug),
  ]) {
    if (slug) sayac.set(slug, (sayac.get(slug) ?? 0) + 1);
  }

  if (sayac.size === 0) return null;

  const adBySlug = new Map(feed.sports.map((x) => [x.slug, x.name]));
  const toplam = [...sayac.values()].reduce((a, b) => a + b, 0);

  const satirlar = [...sayac.entries()]
    .map(([slug, count]) => ({ slug, count, name: adBySlug.get(slug) ?? slug }))
    .sort((a, b) => b.count - a.count);

  return (
    <div className="rounded-2xl border border-line bg-surface p-5">
      <h2 className="font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
        Akışta ne var
      </h2>

      <ul className="mt-4 space-y-3">
        {satirlar.map((satir) => (
          <li key={satir.slug}>
            <div className="flex items-baseline justify-between gap-3">
              <Link
                href={`/${satir.slug}`}
                className="font-display text-xs font-bold uppercase tracking-widest text-chalk underline-offset-4 outline-none hover:text-amber hover:underline focus-visible:underline"
              >
                {satir.name}
              </Link>
              <span className="font-display text-xs tabular-nums text-fog">{satir.count}</span>
            </div>
            <div className="mt-1.5 h-1 overflow-hidden rounded-full bg-raised">
              <span
                className="block h-full rounded-full bg-amber/70"
                style={{ width: `${Math.round((satir.count / toplam) * 100)}%` }}
              />
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
