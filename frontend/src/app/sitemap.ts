import type { MetadataRoute } from "next";
import { getStories } from "@/lib/api";
import { getCommentators } from "@/lib/commentary";
import { getSports } from "@/lib/sports-nav";

const SITE_URL = process.env.SITE_URL ?? "http://localhost:3000";

export const revalidate = 3600;

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const [{ items }, commentators, sports] = await Promise.all([
    getStories({ pageSize: 50 }),
    getCommentators(),
    getSports(true),
  ]);

  const stories = items
    .filter((story) => story.slug)
    .map((story) => ({
      url: `${SITE_URL}/haber/${story.slug}`,
      lastModified: new Date(story.updatedAt),
      changeFrequency: "hourly" as const,
      priority: 0.8,
    }));

  return [
    {
      url: SITE_URL,
      lastModified: new Date(),
      changeFrequency: "hourly",
      priority: 1,
    },
    ...sports.map((sport) => ({
      url: `${SITE_URL}/${sport.slug}`,
      lastModified: new Date(),
      changeFrequency: "hourly" as const,
      priority: 0.7,
    })),
    {
      url: `${SITE_URL}/branslar`,
      lastModified: new Date(),
      changeFrequency: "daily",
      priority: 0.5,
    },
    {
      url: `${SITE_URL}/yorumcular`,
      lastModified: new Date(),
      changeFrequency: "hourly",
      priority: 0.7,
    },
    ...commentators.map((commentator) => ({
      url: `${SITE_URL}/yorumcu/${commentator.slug}`,
      lastModified: commentator.lastOpinionAt ? new Date(commentator.lastOpinionAt) : new Date(),
      changeFrequency: "daily" as const,
      priority: 0.6,
    })),
    ...stories,
  ];
}
