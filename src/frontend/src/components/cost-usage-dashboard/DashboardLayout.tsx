"use client";

import { useState, useEffect, useCallback } from "react";
import TimeRangeSelector, { TIME_RANGES } from "./TimeRangeSelector";
import TotalCostPanel from "./TotalCostPanel";
import CostOverTimePanel from "./CostOverTimePanel";
import { fetchTotalCost, fetchCostTrend } from "@/api/si-01/costUsageClient";
import type {
  PanelState,
  CostUsageResponse,
  CostTrendResponse,
} from "@/types/cost-usage-dashboard";

function initialPanelState<T>(): PanelState<T> {
  return { status: "loading", data: null };
}

function toPanelState<T>(data: T | null, status: string, error?: string): PanelState<T> {
  if (error) return { status: "error", data: null, error };
  if (data === null) return { status: "empty", data: null };
  if (status === "no_samples_in_window") return { status: "empty", data };
  if (status === "incomplete_missing_baseline" || status === "anomaly_counter_decrease") {
    return { status: "incomplete", data };
  }
  return { status: "success", data };
}

export default function DashboardLayout() {
  const defaultRange = TIME_RANGES[0];
  const end = new Date();
  const start = new Date(end.getTime() - defaultRange.days * 24 * 60 * 60 * 1000);

  const [selectedRange, setSelectedRange] = useState(defaultRange.key);
  const [rangeStart, setRangeStart] = useState(start.toISOString());
  const [rangeEnd, setRangeEnd] = useState(end.toISOString());

  const [totalCostState, setTotalCostState] = useState<PanelState<CostUsageResponse>>(
    initialPanelState()
  );
  const [trendState, setTrendState] = useState<PanelState<CostTrendResponse>>(
    initialPanelState()
  );

  const loadData = useCallback(async (s: string, e: string) => {
    setTotalCostState(initialPanelState());
    setTrendState(initialPanelState());

    try {
      const [costResult, trendResult] = await Promise.all([
        fetchTotalCost(s, e),
        fetchCostTrend(s, e),
      ]);

      setTotalCostState(toPanelState(costResult, costResult.aggregationStatus));
      setTrendState(toPanelState(trendResult, "success"));
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Unknown error";
      setTotalCostState((prev) =>
        prev.status === "loading" ? { status: "error", data: null, error: msg } : prev
      );
      setTrendState((prev) =>
        prev.status === "loading" ? { status: "error", data: null, error: msg } : prev
      );
    }
  }, []);

  useEffect(() => {
    loadData(rangeStart, rangeEnd);
  }, [rangeStart, rangeEnd, loadData]);

  const handleRangeChange = (key: string, s: string, e: string) => {
    setSelectedRange(key);
    setRangeStart(s);
    setRangeEnd(e);
  };

  return (
    <div className="max-w-5xl mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Cost Usage Dashboard</h1>
          <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-1">
            OpenCode cost usage from opencode.cost.usage
          </p>
        </div>
        <TimeRangeSelector selected={selectedRange} onChange={handleRangeChange} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-1">
          <TotalCostPanel state={totalCostState} />
        </div>
        <div className="lg:col-span-2">
          <CostOverTimePanel state={trendState} />
        </div>
      </div>
    </div>
  );
}
