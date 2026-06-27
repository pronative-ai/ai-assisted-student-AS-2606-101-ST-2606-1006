import type { CostUsageResponse, CostTrendResponse } from "@/types/cost-usage-dashboard";

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "https://ca-as-2606-101-st-2606-1006--0000002.agreeablegrass-6c3c37bc.centralindia.azurecontainerapps.io";

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
