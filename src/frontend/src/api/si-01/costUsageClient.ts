import type { CostUsageResponse, CostTrendResponse } from "@/types/cost-usage-dashboard";

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5067";

function buildUrl(path: string, params: Record<string, string>): string {
  const query = new URLSearchParams(params).toString();
  return `${API_BASE}${path}?${query}`;
}

export async function fetchTotalCost(start: string, end: string): Promise<CostUsageResponse> {
  const url = buildUrl("/api/opencode/cost-usage", { start, end });
  const res = await fetch(url);
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`Total cost API error (${res.status}): ${body}`);
  }
  return res.json();
}

export async function fetchCostTrend(start: string, end: string): Promise<CostTrendResponse> {
  const url = buildUrl("/api/opencode/cost-usage/trend", { start, end });
  const res = await fetch(url);
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`Cost trend API error (${res.status}): ${body}`);
  }
  return res.json();
}
